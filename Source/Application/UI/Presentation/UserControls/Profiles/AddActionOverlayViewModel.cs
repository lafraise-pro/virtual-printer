﻿using pdfforge.Obsidian;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.UI.Presentation.Helper.ActionHelper;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.WorkflowEditor;
using pdfforge.PDFCreator.UI.Presentation.ViewModelBases;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles
{
    public class AddActionOverlayViewModel : OverlayViewModelBase<AddActionOverlayInteraction, AddActionOverlayViewTranslation>, IMountable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISelectedProfileProvider _selectedProfileProvider;

        public bool ShowInfoText { get; set; }
        public string InfoText { get; set; }
        public ICommand OkCommand { get; }
        public ICommand InfoActionCommand { get; set; }
        public ICommand HideInfoActionCommand { get; set; }
        public ICommand TriggerAddActionCommand { get; set; }
        private IAsyncCommand AddActionCommand { get; set; }
        public ObservableCollection<IPresenterActionFacade> PreparationActions { get; set; }
        public ObservableCollection<IPresenterActionFacade> ModifyActions { get; set; }
        public ObservableCollection<IPresenterActionFacade> SendActions { get; set; }
        private IList<IPresenterActionFacade> ActionFacades { get; set; }

        public AddActionOverlayViewModel(IEventAggregator eventAggregator, ISelectedProfileProvider selectedProfileProvider,
            IEnumerable<IPresenterActionFacade> actionFacades, ITranslationUpdater translationUpdater, ICommandLocator commandLocator)
            : base(translationUpdater)
        {
            _eventAggregator = eventAggregator;
            _selectedProfileProvider = selectedProfileProvider;
            ActionFacades = actionFacades.ToList();
            _settingIndex = SetupSettingsIndex(ActionFacades);

            InfoActionCommand = new DelegateCommand(ShowActionInfo);
            HideInfoActionCommand = new DelegateCommand(HideActionInfo);
            AddActionCommand = commandLocator.GetCommand<AddActionCommand>() as IAsyncCommand;
            TriggerAddActionCommand = new AsyncCommand(TriggerAddActionExecute, TriggerAddActionCanExecute);

            GenerateCollectionViewsOfActions();

            OkCommand = new DelegateCommand((x) =>
            {
                Interaction.Success = true;
                FinishInteraction();
            });
        }

        private readonly Dictionary<string, int> _settingIndex;

        private Dictionary<string, int> SetupSettingsIndex(IEnumerable<IPresenterActionFacade> actionFacades)
        {
            var index = 1;

            var actionList = actionFacades
                .Select(a => a.SettingsType.Name)
                .ToArray();

            return actionList.ToDictionary(x => x, x => index++);
        }

        private int GetSettingsIndex(string settingsName)
        {
            if (_settingIndex.TryGetValue(settingsName, out var index))
                return index;

            return int.MaxValue;
        }

        private void HideActionInfo(object obj)
        {
            ShowInfoText = false;
            RaisePropertyChanged(nameof(ShowInfoText));
        }

        private void ShowActionInfo(object obj)
        {
            if (obj is IPresenterActionFacade presenterFacade)
                InfoText = presenterFacade.InfoText;

            ShowInfoText = true;
            RaisePropertyChanged(nameof(InfoText));
            RaisePropertyChanged(nameof(ShowInfoText));
        }

        private static bool TriggerAddActionCanExecute(object parameter)
        {
            var actionFacade = (IPresenterActionFacade)parameter;
            return !actionFacade.IsRestricted && !actionFacade.IsEnabled;
        }

        private async Task TriggerAddActionExecute(object obj)
        {
            FinishInteraction();
            await AddActionCommand?.ExecuteAsync(obj);
        }

        private void ProfileProviderOnSelectedProfileChanged(object sender, PropertyChangedEventArgs e)
        {
            _eventAggregator.GetEvent<ActionAddedToWorkflowEvent>().Publish();
        }

        private ObservableCollection<IPresenterActionFacade> GetObservableActionsList<TAction>(IEnumerable<IPresenterActionFacade> actions) where TAction : IAction
        {
            var presenterActionFacades = actions.Where(FilterActionFacadeByType<TAction>()).ToList();
            presenterActionFacades.Sort(((x, y) => GetSettingsIndex(x.SettingsType.Name) - GetSettingsIndex(x.SettingsType.Name)));
            return presenterActionFacades.ToObservableCollection();
        }

        private void GenerateCollectionViewsOfActions()
        {
            var actions = ActionFacades.ToList();

            PreparationActions = GetObservableActionsList<IPreConversionAction>(actions);
            ModifyActions = GetObservableActionsList<IConversionAction>(actions);
            SendActions = GetObservableActionsList<IPostConversionAction>(actions);

            RaisePropertyChanged(nameof(PreparationActions));
            RaisePropertyChanged(nameof(ModifyActions));
            RaisePropertyChanged(nameof(SendActions));
        }

        private Func<IPresenterActionFacade, bool> FilterActionFacadeByType<TType>() where TType : IAction
        {
            return x => x.ActionType.GetInterfaces().Contains(typeof(TType));
        }

        public override string Title => $"{Translation.AddAction}";

        public void MountView()
        {
            _selectedProfileProvider.SelectedProfileChanged += ProfileProviderOnSelectedProfileChanged;
        }

        public void UnmountView()
        {
            _selectedProfileProvider.SelectedProfileChanged -= ProfileProviderOnSelectedProfileChanged;
        }
    }
}
