﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using pdfforge.Obsidian;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.GroupPolicies;
using pdfforge.PDFCreator.Core.ServiceLocator;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.Core.SettingsManagementInterface;
using pdfforge.PDFCreator.UI.Presentation.Helper;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.ViewModelBases;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.DefaultViewerSettings
{
    public class DefaultViewerViewModel : TranslatableViewModelBase<DefaultViewerTranslation>, IWhitelisted, IMountable
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly ICurrentSettingsProvider _currentSettingsProvider;
        private readonly IOpenFileInteractionHelper _fileInteractionHelper;
        public IGpoSettings GpoSettings { get; }

        private ObservableCollection<DefaultViewer> _defaultViewers => _settingsProvider?.Settings.DefaultViewerList;

        public DefaultViewerViewModel(
            ITranslationUpdater translationUpdater,
            ISettingsProvider settingsProvider,
            ICurrentSettingsProvider currentSettingsProvider,
            IGpoSettings gpoSettings, 
            IOpenFileInteractionHelper fileInteractionHelper)
            : base(translationUpdater)
        {
            _settingsProvider = settingsProvider;
            _currentSettingsProvider = currentSettingsProvider;

           
            _fileInteractionHelper = fileInteractionHelper;
            GpoSettings = gpoSettings;

            if (_defaultViewers != null)
                UpdateDefaultViewer();

            FindPathCommand = new DelegateCommand(ExecuteFindPath);

        }

        public ICommand FindPathCommand { get; set; }

        private void UpdateDefaultViewer()
        {
            RaisePropertyChanged(nameof(DefaultViewers));
        }

        public void ExecuteFindPath(object data)
        {
            var model = (DefaultViewer)data;
            var filter = Translation.ExecutableFiles
                         + @" (*.exe, *.bat, *.cmd)|*.exe;*.bat;*.cmd|"
                         + Translation.AllFiles
                         + @"(*.*)|*.*";

            var interactionResult = _fileInteractionHelper.StartOpenFileInteraction("", "", filter);
            interactionResult.MatchSome(s =>
            {
                model.Path = s;
                RaisePropertyChanged(nameof(DefaultViewers));
            });
        }

        public ObservableCollection<DefaultViewer> DefaultViewers
        {
            get { return _defaultViewers; }
        }

        protected override void OnTranslationChanged()
        {
            base.OnTranslationChanged();
        }

        public void MountView()
        {
            _currentSettingsProvider.SettingsChanged += OnSettingsChanged;
            UpdateDefaultViewer();
        }

        private void OnSettingsChanged(object sender, EventArgs args)
        {
            UpdateDefaultViewer();
        }

        public void UnmountView()
        {
            _currentSettingsProvider.SettingsChanged -= OnSettingsChanged;
        }
    }
}