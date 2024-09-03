﻿using pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.DebugSettings;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.DefaultViewerSettings;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.DirectConversion;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.General;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Settings.TitleReplacementSettings;

namespace pdfforge.PDFCreator.UI.Presentation
{
    public static partial class RegionViewName
    {
        public static string AboutView => nameof(UserControls.AboutView);
        public static string AccountsView => nameof(UserControls.Accounts.AccountsView);
        public static string ArchitectView => nameof(UserControls.Architect.ArchitectView);
        public static string HomeView => nameof(UserControls.Home.HomeView);
        public static string PrinterView => nameof(UserControls.Printer.PrinterView);
        public static string ProfilesView => nameof(UserControls.Profiles.ProfilesView);
        public static string ApplicationSettingsView => nameof(UserControls.Settings.ApplicationSettingsView);
        public static string GeneralSettingsRegionView => nameof(UserControls.Settings.GeneralSettingsView);
        public static string GeneralSettings => nameof(GeneralSettingsView);
        public static string DebugSettingRegionView => nameof(DebugSettingView);
        public static string TitleReplacementsRegionView => nameof(TitleReplacementsView);
        public static string DefaultViewerRegionView => nameof(DefaultViewerView);
        public static string DirectImageConversionSettingsRegionView => nameof(DirectImageConversionSettingView);
    }
}
