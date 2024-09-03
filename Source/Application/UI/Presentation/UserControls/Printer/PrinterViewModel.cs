﻿using pdfforge.Obsidian;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.GroupPolicies;
using pdfforge.PDFCreator.Core.Printing.Printer;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.Core.SettingsManagementInterface;
using pdfforge.PDFCreator.UI.Presentation.Assistants;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles;
using pdfforge.PDFCreator.UI.Presentation.ViewModelBases;
using pdfforge.PDFCreator.UI.Presentation.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NLog;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Printer
{
    public class PrinterViewModel : TranslatableViewModelBase<PrinterTabTranslation>, IMountable
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        private readonly ConversionProfileWrapper _dummyLastUsedProfile = new ConversionProfileWrapper
        (
            new ConversionProfile()
            {
                Name = "<Last used profile>",
                Guid = ProfileGuids.LAST_USED_PROFILE_GUID
            });

        private readonly IPrinterAssistant _printerAssistant;
        private readonly IPrinterHelper _printerHelper;
        private readonly IGpoSettings _gpoSettings;

        private readonly IPrinterProvider _printerProvider;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ICurrentSettings<ObservableCollection<PrinterMapping>> _printerMappingProvider;
        private ConversionProfileWrapper _defaultProfile;

        private ICollection<string> _pdfCreatorPrinters;
        protected Helper.SynchronizedCollection<PrinterMappingWrapper> _printerMappings;
        private ICollectionView _printerMappingView;

        protected readonly ICurrentSettings<ObservableCollection<ConversionProfile>> ProfilesProvider;

        public PrinterViewModel(
            IPrinterProvider printerProvider,
            ISettingsProvider settingsProvider,
            ICurrentSettings<ObservableCollection<PrinterMapping>> printerMappingProvider,
            ICurrentSettings<ObservableCollection<ConversionProfile>> profilesProvider,
            IPrinterAssistant printerAssistant,
            ITranslationUpdater translationUpdater,
            IPrinterHelper printerHelper,
            IGpoSettings gpoSettings)
            : base(translationUpdater)
        {
            _printerHelper = printerHelper;
            _gpoSettings = gpoSettings;
            _printerAssistant = printerAssistant;
            _printerProvider = printerProvider;
            _settingsProvider = settingsProvider;
            _printerMappingProvider = printerMappingProvider;
            ProfilesProvider = profilesProvider;

            AddPrinterCommand = new DelegateCommand(AddPrintercommandExecute);
            RenamePrinterCommand = new DelegateCommand(RenamePrinterCommandExecute, ModifyPrinterCommandCanExecute);
            DeletePrinterCommand = new DelegateCommand(DeletePrinterCommandExecute, ModifyPrinterCommandCanExecute);
            SetPrimaryPrinterCommand = new DelegateCommand(SetPrimaryPrinter);
        }

        public void MountView()
        {
            SetSettingsAndRaiseNotifications(ProfilesProvider.Settings);
            _settingsProvider.SettingsChanged += SettingsProviderOnSettingsChanged;
        }

        public void UnmountView()
        {
            _settingsProvider.SettingsChanged -= SettingsProviderOnSettingsChanged;
            _printerMappings.ObservableCollection.CollectionChanged -= PrinterMappings_OnCollectionChanged;
            _printerMappingView.CurrentChanged -= PrinterMappingView_OnCurrentChanged;
        }

        private void SettingsProviderOnSettingsChanged(object sender, EventArgs eventArgs)
        {
            SetSettingsAndRaiseNotifications(ProfilesProvider.Settings);
        }

        private void SetPrimaryPrinter(object parameter)
        {
            var selectedPrinter = parameter as PrinterMappingWrapper;

            if (selectedPrinter == null)
                return;

            PrimaryPrinter = selectedPrinter.PrinterName;
        }

        private ObservableCollection<ConversionProfileWrapper> _conversionProfiles;

        public ObservableCollection<ConversionProfileWrapper> ConversionProfiles
        {
            get
            {
                if (_conversionProfiles == null)
                {
                    _conversionProfiles = _settingsProvider.Settings?.Copy().ConversionProfiles.Select(x => new ConversionProfileWrapper(x)).ToObservableCollection();
                    _conversionProfiles?.Insert(0, _dummyLastUsedProfile);
                }

                return _conversionProfiles;
            }
            set
            {
                if (value == null)
                    return;

                _conversionProfiles = value;
                _conversionProfiles?.Insert(0, _dummyLastUsedProfile);

                var buffer = new Dictionary<string, string>();
                if (PrinterMappings != null)
                {
                    foreach (var printerMappingWrapper in PrinterMappings)
                    {
                        buffer.Add(printerMappingWrapper.PrinterName, printerMappingWrapper.Profile.ConversionProfile.Guid);
                    }
                }

                RaisePropertyChanged(nameof(ConversionProfiles));

                if (PrinterMappings != null)
                {
                    foreach (var printerMappingWrapper in PrinterMappings)
                    {
                        printerMappingWrapper.Profile = _conversionProfiles.FirstOrDefault(x => x.ConversionProfile.Guid == buffer[printerMappingWrapper.PrinterName]);
                    }
                }

                _defaultProfile = ConversionProfiles.FirstOrDefault(x => x.ConversionProfile.Guid == ProfileGuids.DEFAULT_PROFILE_GUID);
                if (_defaultProfile == null)
                    _defaultProfile = _dummyLastUsedProfile;
            }
        }

        public IEnumerable<ConversionProfileWrapper> PrinterMappingProfiles
        {
            get
            {
                var profiles = ConversionProfiles.ToList();
                _dummyLastUsedProfile.ConversionProfile.Name = "<" + Translation.LastUsedProfileMapping + ">";
                profiles.Insert(0, _dummyLastUsedProfile);
                return profiles;
            }
        }

        public ICollection<string> PdfCreatorPrinters
        {
            get { return _pdfCreatorPrinters; }
            set
            {
                _pdfCreatorPrinters = value;
                RaisePropertyChanged(nameof(PdfCreatorPrinters));
            }
        }

        public ObservableCollection<PrinterMappingWrapper> PrinterMappings
        {
            get { return _printerMappings?.ObservableCollection; }
        }

        public ICommand AddPrinterCommand { get; private set; }
        public ICommand RenamePrinterCommand { get; }
        public ICommand DeletePrinterCommand { get; }
        public DelegateCommand SetPrimaryPrinterCommand { get; }

        public string PrimaryPrinter
        {
            get
            {
                if (string.IsNullOrEmpty(_settingsProvider.Settings.CreatorAppSettings.PrimaryPrinter) ||
                    PrinterMappings.All(o => o.PrinterName != _settingsProvider.Settings.CreatorAppSettings.PrimaryPrinter))
                {
                    _settingsProvider.Settings.CreatorAppSettings.PrimaryPrinter = _printerHelper.GetApplicablePDFCreatorPrinter("PDFCreator",
                        "PDFCreator");
                }

                return _settingsProvider.Settings.CreatorAppSettings.PrimaryPrinter;
            }
            set
            {
                _settingsProvider.Settings.CreatorAppSettings.PrimaryPrinter = value;
                UpdatePrimaryPrinter(value);
                RaisePropertyChanged(nameof(PrimaryPrinter));
            }
        }

        private void SetSettingsAndRaiseNotifications(ObservableCollection<ConversionProfile> profiles)
        {
            _logger.Debug("SetSettingsAndRaiseNotifications");

            ConversionProfiles = profiles.Select(x => new ConversionProfileWrapper(x)).ToObservableCollection();

            RaisePropertyChanged(nameof(ApplicationSettings));

            UpdatePrinterList();
            ApplyPrinterMappings();
            UpdatePrinterCollectionViews();

            UpdatePrimaryPrinter(PrimaryPrinter);
            RaisePropertyChanged(nameof(PrimaryPrinter));
        }

        private void PrinterMappings_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _printerMappingProvider.Settings.Clear();

            foreach (var printerMappingWrapper in PrinterMappings)
            {
                _printerMappingProvider.Settings.Add(printerMappingWrapper.PrinterMapping);
                if (printerMappingWrapper.Profile == null)
                    printerMappingWrapper.Profile = _defaultProfile;
            }

            RaisePrinterCommandsCanExecuteChanged();
        }

        private void PrinterMappingView_OnCurrentChanged(object sender, EventArgs eventArgs)
        {
            RaisePrinterCommandsCanExecuteChanged();
        }

        private void RaisePrinterCommandsCanExecuteChanged()
        {
            var delegateCommand = RenamePrinterCommand as DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            var command = DeletePrinterCommand as DelegateCommand;
            command?.RaiseCanExecuteChanged();
        }

        private void UpdatePrinterList()
        {
            _logger.Debug("UpdatePrinterList");

            if (_pdfCreatorPrinters == null)
                _pdfCreatorPrinters = new List<string>();

            _pdfCreatorPrinters.Clear();
            foreach (var printer in _printerProvider.GetPDFCreatorPrinters())
            {
                _logger.Debug("Installed Printer: " + printer);
                _pdfCreatorPrinters.Add(printer);
            }

            RaisePropertyChanged(nameof(PdfCreatorPrinters));
        }

        public void UpdatePrimaryPrinter(string printerName)
        {
            foreach (var printerMappingWrapper in _printerMappings.ObservableCollection)
            {
                printerMappingWrapper.PrimaryPrinter = printerName;
            }
        }

        private void ApplyPrinterMappings()
        {
            _logger.Debug("ApplyPrinterMappings");

            if (_printerMappingProvider?.Settings != null)
            {
                _logger.Debug("Printer mappings from registry:");
                foreach (var pm in _printerMappingProvider.Settings)
                {
                    _logger.Debug("Printer Mapping: " + pm.PrinterName + " - " + pm.ProfileGuid);
                }

                var mappingWrappers = new List<PrinterMappingWrapper>();

                foreach (var printerMapping in _printerMappingProvider.Settings)
                {
                    var mappingWrapper = new PrinterMappingWrapper(printerMapping, PrinterMappingProfiles);
                    if (mappingWrapper.Profile == null)
                    {
                        _logger.Debug("Could not find profile for " + mappingWrapper.PrinterName + ". Default Profile is set");
                        mappingWrapper.Profile = _defaultProfile;
                    }
                    mappingWrappers.Add(mappingWrapper);
                }

                _printerMappings = new Helper.SynchronizedCollection<PrinterMappingWrapper>(mappingWrappers);

                _logger.Debug("List Printer Mappings:");
                foreach (var pm in PrinterMappings)
                {
                    _logger.Debug("Printer Mapping: " + pm.PrinterName + " - " + pm.Profile.Name);
                }

                _printerMappings.ObservableCollection.CollectionChanged += PrinterMappings_OnCollectionChanged;
                _printerMappingView = CollectionViewSource.GetDefaultView(_printerMappings.ObservableCollection);
                _printerMappingView.CurrentChanged += PrinterMappingView_OnCurrentChanged;
            }

            RaisePropertyChanged(nameof(PrinterMappings));
        }

        private bool ModifyPrinterCommandCanExecute(object o)
        {
            var currentMapping = _printerMappingView?.CurrentItem as PrinterMappingWrapper;

            if (currentMapping == null)
                return false;

            return PdfCreatorPrinters.Contains(currentMapping.PrinterName);
        }

        private async void AddPrintercommandExecute(object o)
        {
            string printerName = await _printerAssistant.AddPrinter();

            if (string.IsNullOrEmpty(printerName))
                return;

            if (!string.IsNullOrWhiteSpace(printerName))
            {
                var newMapping = new PrinterMappingWrapper(new PrinterMapping(printerName, ProfileGuids.DEFAULT_PROFILE_GUID), ConversionProfiles);

                var duplicateMapping = PrinterMappings.FirstOrDefault(p => p.PrinterName == newMapping.PrinterName);

                if (duplicateMapping == null)
                {
                    PrinterMappings.Add(newMapping);
                }
                else
                {
                    duplicateMapping.Profile = newMapping.Profile;
                    duplicateMapping.PrinterMapping.ProfileGuid = newMapping.PrinterMapping.ProfileGuid;
                }
            }

            UpdatePrinterCollectionViews();
        }

        private async void RenamePrinterCommandExecute(object obj)
        {
            var currentMapping = _printerMappingView.CurrentItem as PrinterMappingWrapper;

            if (currentMapping == null)
                return;

            var wasPrimaryPrinter = currentMapping.IsPrimaryPrinter;

            string newPrinterName = await _printerAssistant.RenamePrinter(currentMapping.PrinterName);

            if (newPrinterName != null)
            {
                PdfCreatorPrinters = _printerProvider.GetPDFCreatorPrinters();
                currentMapping.PrinterName = newPrinterName;
                if (wasPrimaryPrinter)
                    PrimaryPrinter = newPrinterName;
            }

            RaisePropertyChanged(nameof(PdfCreatorPrinters));
            RaisePropertyChanged(nameof(PrimaryPrinter));
        }

        private async void DeletePrinterCommandExecute(object obj)
        {
            var currentMapping = _printerMappingView.CurrentItem as PrinterMappingWrapper;

            if (currentMapping == null)
                return;

            var success = await _printerAssistant.DeletePrinter(currentMapping.PrinterName, PdfCreatorPrinters.Count);

            if (success)
            {
                PrinterMappings.Remove(currentMapping);
                RaisePropertyChanged(nameof(PrinterMappings));
                PdfCreatorPrinters = _printerProvider.GetPDFCreatorPrinters();
                RaisePropertyChanged(nameof(PdfCreatorPrinters));

                PrimaryPrinter = _printerHelper.GetApplicablePDFCreatorPrinter(PrimaryPrinter);
            }
        }

        public bool PrinterIsDisabled
        {
            get
            {
                if (ProfilesProvider.Settings == null)
                    return false;

                return _gpoSettings != null && _gpoSettings.DisablePrinterTab;
            }
        }

        private void UpdatePrinterCollectionViews()
        {
            UpdatePrinterList();
            CollectionViewSource.GetDefaultView(_printerMappingProvider.Settings).Refresh();
            CollectionViewSource.GetDefaultView(PdfCreatorPrinters).Refresh();
        }

        protected override void OnTranslationChanged()
        {
            base.OnTranslationChanged();

            if (PrinterMappings != null)
            {
                foreach (var mappingWrapper in PrinterMappings)
                {
                    mappingWrapper.Profile = PrinterMappingProfiles?.FirstOrDefault(x => x.ConversionProfile.Guid == mappingWrapper.Profile.ConversionProfile.Guid);
                }
            }
        }
    }
}
