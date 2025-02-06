namespace pdfforge.PDFCreator.Conversion.Settings.GroupPolicies
{
    public class GpoSettingsDefaults : IGpoSettings
    {
        public bool DisableApplicationSettings => false;
        public bool DisableDebugTab => false;
        public bool DisablePrinterTab => false;
        public bool DisableProfileManagement => false;
        public bool DisableTitleTab => false;
        public bool DisableHistory => true;
        public bool DisableAccountsTab => false;
        public bool DisableUsageStatistics => true;
        public bool DisableRssFeed => true;
        public bool DisableTips => true;
        public bool HideLicenseTab => true;
        public bool HidePdfArchitectInfo => true;
        
        public string Language => null;
        public string UpdateInterval => null;
        public string SharedSettingsFilename => "settings";
        public int? HotStandbyMinutes => null;

        public bool LoadSharedAppSettings => true;
        public bool LoadSharedProfiles => true;
        public bool AllowUserDefinedProfiles => true;

        public bool AllowSharedProfilesEditing => false;
        public bool DisableLicenseExpirationReminder => true;

        public string PageSize => null;
        public string PageOrientation => null;

        public bool HideFeedbackForm => true;
    }
}