﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.GroupPolicies;
using pdfforge.PDFCreator.Core.ServiceLocator;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.UI.Presentation.Commands.TitleReplacements;
using pdfforge.PDFCreator.UI.Presentation.DesignTime;
using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.ViewModelBases;
using Prism.Mvvm;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.TitleReplacementSettings
{
    public class TitleReplacementsViewModel : TranslatableViewModelBase<TitleReplacementsTranslation>, IWhitelisted, IMountable
    {
        private readonly ICurrentSettings<ObservableCollection<TitleReplacement>> _titleReplacementProvider;
        private readonly ICurrentSettingsProvider _settingsProvider;
        public IGpoSettings GpoSettings { get; }

        private string _replacedSampleText;

        //private string _replacementTypeText;
        private string _sampleText = "Microsoft Word - Sample Text.doc";
        
        public TitleReplacementsViewModel(ITranslationUpdater translationUpdater, ICurrentSettings<ObservableCollection<TitleReplacement>> titleReplacementProvider, ICurrentSettingsProvider settingsProvider, ICommandLocator commandLocator, IGpoSettings gpoSettings) : base(translationUpdater)
        {
            _titleReplacementProvider = titleReplacementProvider;
            _settingsProvider = settingsProvider;
            GpoSettings = gpoSettings;
            TitleAddCommand = commandLocator.GetCommand<TitleReplacementAddCommand>();
            TitleDeleteCommand = commandLocator.GetCommand<TitleReplacementRemoveCommand>();
            TitleEditCommand = commandLocator.GetCommand<TitleReplacementEditCommand>();

            if (TitleReplacements != null)
                UpdateTitleReplacements();

        }

        private void UpdateTitleReplacements()
        {
            UpdateReplacedText();
        }

        private void TitleReplacementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateReplacedText();
        }

        public string SampleText
        {
            get { return _sampleText; }
            set
            {
                _sampleText = value;
                UpdateReplacedText();
            }
        }

        public string ReplacedSampleText
        {
            get { return _replacedSampleText; }
            set
            {
                _replacedSampleText = value;
                RaisePropertyChanged();
            }
        }

        public ICommand TitleEditCommand { get; set; }

        public ICommand TitleAddCommand { get; set; }

        public ICommand TitleDeleteCommand { get; set; }

        public ObservableCollection<TitleReplacement> TitleReplacements => _titleReplacementProvider?.Settings;

        private void UpdateReplacedText()
        {
            var titleReplacer = new TitleReplacer();
            titleReplacer.AddReplacements(TitleReplacements);
            ReplacedSampleText = titleReplacer.Replace(SampleText);

            var view = CollectionViewSource.GetDefaultView(TitleReplacements);
            view.SortDescriptions.Add(new SortDescription("ReplacementType", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Search", ListSortDirection.Descending));

            RaisePropertyChanged(nameof(TitleReplacements));
        }

        public void MountView()
        {
            _settingsProvider.SettingsChanged += OnSettingsChanged;

            if(TitleReplacements != null)
                TitleReplacements.CollectionChanged += TitleReplacementsOnCollectionChanged;
        }
        public void UnmountView()
        {
            _settingsProvider.SettingsChanged -= OnSettingsChanged;

            if(TitleReplacements != null)
                TitleReplacements.CollectionChanged -= TitleReplacementsOnCollectionChanged;
        }

        private void OnSettingsChanged(object sender, EventArgs args)
        {
            UpdateTitleReplacements();
        }

    }
}