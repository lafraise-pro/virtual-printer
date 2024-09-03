﻿using pdfforge.Obsidian;
using pdfforge.PDFCreator.Core.Controller;
using pdfforge.PDFCreator.Core.ServiceLocator;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.UI.Presentation.Assistants;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.ViewModelBases;
using pdfforge.PDFCreator.Utilities.Web;
using System.Windows.Input;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls
{
    public class LicenseExpirationReminderViewModel : TranslatableViewModelBase<LicenseExpirationReminderTranslation>, IWhitelisted
    {
        private readonly ILicenseExpirationReminder _licenseExpirationReminder;
        private readonly IWebLinkLauncher _webLinkLauncher;
        private readonly string _licenseKey;
        public ICommand RemindMeLaterCommand { get; }
        public ICommand ManageLicenseCommand { get; }
        public bool ShowLicenseExpireReminder { get; set; }

        public LicenseExpirationReminderViewModel(ILicenseExpirationReminder licenseExpirationReminder, ICommandLocator commandLocator,
            ITranslationUpdater translationUpdater, IWebLinkLauncher webLinkLauncher) : base(translationUpdater)
        {
            _licenseExpirationReminder = licenseExpirationReminder;
            _webLinkLauncher = webLinkLauncher;
            _licenseKey = _licenseExpirationReminder.LicenseKey;

            ManageLicenseCommand = commandLocator?.CreateMacroCommand()
                .AddCommand(new DelegateCommand(_ => ManageLicenseCommandExecute()))
                .AddCommand(new DelegateCommand(_ => SetReminderForLicenseExpiration()))
                .Build();

            ShowLicenseExpireReminder = _licenseExpirationReminder.IsExpirationReminderDue();

            RemindMeLaterCommand = new DelegateCommand(o => SetReminderForLicenseExpiration());
        }

        private void ManageLicenseCommandExecute()
        {
            _webLinkLauncher.Launch($"{Urls.LicenseServerManageSingleLicense}?license_key={_licenseKey}");
        }

        public string LicenseReminderInfo =>
            _licenseExpirationReminder.DaysTillLicenseExpires >= 0
                ? Translation.FormatLicenseExpiryDate(_licenseExpirationReminder.DaysTillLicenseExpires)
                : Translation.LicenseHasExpired;

        private void SetReminderForLicenseExpiration()
        {
            _licenseExpirationReminder.SetReminderForLicenseExpiration();
            ShowLicenseExpireReminder = false;
            RaisePropertyChanged(nameof(ShowLicenseExpireReminder));
        }
    }
}
