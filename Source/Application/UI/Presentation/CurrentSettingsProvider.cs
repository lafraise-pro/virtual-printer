﻿using pdfforge.Obsidian;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Settings;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using pdfforge.PDFCreator.Core.SettingsManagementInterface;
using NaturalSort.Extension;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles;

namespace pdfforge.PDFCreator.UI.Presentation
{
    public interface ISelectedProfileProvider
    {
        ConversionProfile SelectedProfile { get; set; }

        ConversionProfile GetProfileByName(string name);

        event PropertyChangedEventHandler SelectedProfileChanged;

        event EventHandler SettingsChanged;

        int GetRegisteredSelectedProfileChangedListener();
    }

    public interface ICurrentSettingsProvider : ISelectedProfileProvider
    {
        void StoreCurrentSettings();

        void Reset(bool fullClone);

        CurrentCheckSettings CheckSettings { get; }
    }

    public class CurrentSettingsProvider : ObservableObject, ICurrentSettingsProvider
    {
        private readonly ISettingsProvider _settingsProvider;
        private ConversionProfile _selectedProfile;
        private PdfCreatorSettings _settings;

        public CurrentSettingsProvider(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _settingsProvider.SettingsChanged += (sender, args) =>
            {
                UpdateSettings(true);
            };
        }

        public PdfCreatorSettings Settings
        {
            get
            {
                if (_settings == null)
                    UpdateSettings(false);
                return _settings;
            }
        }

        public Accounts Accounts => Settings.ApplicationSettings.Accounts;

        public ObservableCollection<ConversionProfile> Profiles => Settings?.ConversionProfiles;

        public ObservableCollection<TitleReplacement> TitleReplacements => Settings.ApplicationSettings.TitleReplacement;
        public ObservableCollection<PrinterMapping> PrinterMappings => Settings.ApplicationSettings.PrinterMappings;

        public CurrentCheckSettings CheckSettings => new CurrentCheckSettings(Profiles, PrinterMappings, Accounts);

        public ConversionProfile SelectedProfile
        {
            get
            {
                if (_selectedProfile == null)
                    UpdateSettings(false);
                return _selectedProfile;
            }
            set
            {
                if (value == null)
                    return;
                _selectedProfile = value;
                RaisePropertyChanged(nameof(SelectedProfile));
                SelectedProfileChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProfile)));
            }
        }

        public ConversionProfile GetProfileByName(string name)
        {
            return Profiles.FirstOrDefault(p => p.Name == name);
        }

        private void OverrideDefaultViewerList(ObservableCollection<DefaultViewer> target, ObservableCollection<DefaultViewer> source)
        {
            foreach (var targetView in target)
            {
                var sourceView = source.FirstOrDefault(viewer => viewer.OutputFormat == targetView.OutputFormat);
                if (sourceView == null)
                    continue;

                targetView.IsActive = sourceView.IsActive;
                targetView.Parameters = sourceView.Parameters;
                targetView.Path = sourceView.Path;
            }
        }

        public void StoreCurrentSettings()
        {
            Settings.ConversionProfiles = Settings.ConversionProfiles
                .OrderBy(p => p.Name, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                .ToObservableCollection();
            Settings.ApplicationSettings.PrinterMappings = Settings.ApplicationSettings.PrinterMappings
                .OrderBy(p => p.PrinterName, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                .ToObservableCollection();

            OverrideDefaultViewerList(Settings.DefaultViewers, _settingsProvider.Settings.DefaultViewers);

            _settingsProvider.UpdateSettings(Settings);
        }

        public void Reset(bool fullClone)
        {
            CloneSettings(fullClone);
            SelectedProfile = _settings.GetProfileByName(_selectedProfile.Name);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(Settings));
            RaisePropertyChanged(nameof(Accounts));
            RaisePropertyChanged(nameof(TitleReplacements));
            RaisePropertyChanged(nameof(PrinterMappings));
        }

        public event EventHandler SettingsChanged;

        public int GetRegisteredSelectedProfileChangedListener()
        {
            return SelectedProfileChanged != null ? SelectedProfileChanged.GetInvocationList().Length : 0;
        }

        public event PropertyChangedEventHandler SelectedProfileChanged;

        private void UpdateSettings(bool forceUpdate)
        {
            if (_settingsProvider?.Settings == null)
                return;

            if (_settings == null || forceUpdate)
            {
                CloneSettings(false);
                var firstProfile = Profiles.FirstOrDefault();
                _selectedProfile = _selectedProfile == null ? firstProfile : Profiles.FirstOrDefault(x => x.Guid == _selectedProfile.Guid) ?? firstProfile;
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CloneSettings(bool fullClone)
        {
            _settings = fullClone
                ? _settingsProvider.Settings.Copy()
                : _settingsProvider.Settings.CopyAndPreserveApplicationSettings();
        }
    }
}
