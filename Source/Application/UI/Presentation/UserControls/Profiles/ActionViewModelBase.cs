﻿using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.Core.Services.Translation;
using pdfforge.PDFCreator.Core.SettingsManagement.DefaultSettings;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using System;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles
{
    public interface IActionViewModel : IStatusHintViewModel, IMountable
    {
        IAction Action { get; }

        string Title { get; }
        bool IsEnabled { get; set; }
        bool IsRestricted { get; }
        AddActionToolTip AddActionToolTip { get; }
        string InfoText { get; }

        IProfileSetting GetCurrentSettingCopy();

        void ReplaceCurrentSetting(IProfileSetting profileSetting);

        void AddAction();

        void RemoveAction();
    }

    public abstract class ActionViewModelBase<TAction, TTranslation> : ProfileUserControlViewModel<TTranslation>, IActionViewModel
        where TAction : class, IAction
        where TTranslation : IActionTranslation, new()
    {
        public virtual bool HideStatusInOverlay => false;

        private readonly ErrorCodeInterpreter _errorCodeInterpreter;
        private readonly ICurrentSettingsProvider _currentSettingsProvider;
        private readonly IDefaultSettingsBuilder _defaultSettingsBuilder;
        private readonly IActionOrderHelper _actionOrderHelper;

        protected ActionViewModelBase(IActionLocator actionLocator,
            ErrorCodeInterpreter errorCodeInterpreter,
            ITranslationUpdater translationUpdater,
            ICurrentSettingsProvider currentSettingsProvider,
            IDispatcher dispatcher,
            IDefaultSettingsBuilder defaultSettingsBuilder,
            IActionOrderHelper actionOrderHelper)
            : base(translationUpdater, currentSettingsProvider, dispatcher)
        {
            Action = actionLocator.GetAction<TAction>();
            _errorCodeInterpreter = errorCodeInterpreter;
            _currentSettingsProvider = currentSettingsProvider;
            _defaultSettingsBuilder = defaultSettingsBuilder;
            _actionOrderHelper = actionOrderHelper;
        }

        public IAction Action { get; }

        public bool IsEnabled
        {
            get => Action.IsEnabled(CurrentProfile);
            set => CurrentSetting.Enabled = value;
        }

        private IProfileSetting CurrentSetting
        {
            get => Action.GetProfileSetting(CurrentProfile);
        }

        public string Title => Translation.Title;
        public string InfoText => Translation.InfoText;
        public bool IsRestricted => Action.IsRestricted(CurrentProfile);

        public AddActionToolTip AddActionToolTip
        {
            get
            {
                if (IsEnabled)
                    return new AddActionToolTip(Translation.EnabledHint, true);
                if (IsRestricted)
                    return new AddActionToolTip(Translation.RestrictedHint, true);
                return new AddActionToolTip(string.Empty, false);
            }
        }

        protected Conversion.Settings.Accounts Accounts => _currentSettingsProvider.CheckSettings.Accounts;

        protected abstract string SettingsPreviewString { get; }

        public string StatusText
        {
            get
            {
                var actionStatus = DetermineActionStatus();
                HasWarning = actionStatus.HasWarning;
                RaisePropertyChanged(nameof(HasWarning));
                return actionStatus.StatusText;
            }
        }

        public bool HasWarning
        { get; private set; }

        private (bool HasWarning, string StatusText) DetermineActionStatus()
        {
            if (CurrentProfile == null)
                return (false, "");
            if (Action.IsRestricted(CurrentProfile))
                return (true, Translation.RestrictedHint);

            var result = Action.Check(CurrentProfile, _currentSettingsProvider.CheckSettings, CheckLevel.EditingProfile);
            if (!result)
                return (true, _errorCodeInterpreter.GetFirstErrorText(result, false));

            return (false, SettingsPreviewString);
        }

        protected void StatusChanged(object sender = null, EventArgs e = null)
        {
            RaisePropertyChanged(nameof(StatusText));
        }

        public void ReplaceCurrentSetting(IProfileSetting value)
        {
            var replaceWithMethod = CurrentSetting?.GetType().GetMethod(nameof(ConversionProfile.ReplaceWith));
            replaceWithMethod?.Invoke(CurrentSetting, new[] { value });
        }

        private void ResetToDefaultSettings()
        {
            var defaultProfile = _defaultSettingsBuilder.CreateDefaultProfile();
            var defaultSetting = Action.GetProfileSetting(defaultProfile);
            ReplaceCurrentSetting(defaultSetting);
        }

        public void RemoveAction()
        {
            ResetToDefaultSettings();
            Action.GetProfileSetting(CurrentProfile).Enabled = false;
            CurrentProfile.ActionOrder.RemoveAll(x => x == Action.SettingsType.Name);
        }

        public IProfileSetting GetCurrentSettingCopy()
        {
            var copyMethod = CurrentSetting?.GetType().GetMethod(nameof(ConversionProfile.Copy));
            return (IProfileSetting)copyMethod?.Invoke(CurrentSetting, null);
        }

        public void AddAction()
        {
            ResetToDefaultSettings();
            Action.GetProfileSetting(CurrentProfile).Enabled = true;
            CurrentProfile.ActionOrder.Add(Action.SettingsType.Name);
            _actionOrderHelper.EnsureValidOrder(CurrentProfile.ActionOrder);
        }

        public override void MountView()
        {
            base.MountView();
            CurrentSetting.PropertyChanged += StatusChanged;
        }

        public override void UnmountView()
        {
            base.UnmountView();
            CurrentSetting.PropertyChanged -= StatusChanged;
        }
    }
}
