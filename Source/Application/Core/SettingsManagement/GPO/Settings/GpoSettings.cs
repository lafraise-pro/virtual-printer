using pdfforge.DataStorage.Storage;
using pdfforge.DataStorage;
using System.Collections.Generic;
using System.Text;
using System;

// ! This file is generated automatically.
// ! Do not edit it outside the sections for custom code.
// ! These changes will be deleted during the next generation run

namespace pdfforge.PDFCreator.Core.SettingsManagement.GPO.Settings
{
	public partial class GpoSettings {
		
		public ApplicationSettings ApplicationSettings { get; set; } = new ApplicationSettings();
		
		public CreatorAppSettings CreatorAppSettings { get; set; } = new CreatorAppSettings();
		
		/// <summary>
		/// Enable to allows own user profiles next to the shared profiles
		/// </summary>
		public bool AllowUserDefinedProfiles { get; set; } = true;
		
		/// <summary>
		/// Activate to disable accounts tab. This disables the functionality to add, remove or change accounts.
		/// </summary>
		public bool DisableAccountsTab { get; set; } = false;
		
		/// <summary>
		/// Activate to disable the access to the whole application settings.
		/// </summary>
		public bool DisableApplicationSettings { get; set; } = false;
		
		/// <summary>
		/// Activate to disable debug tab. This disables the functionality to run test jobs and change the debug level.
		/// </summary>
		public bool DisableDebugTab { get; set; } = false;
		
		/// <summary>
		/// Activate to disable the job history.
		/// </summary>
		public bool DisableHistory { get; set; } = false;
		
		/// <summary>
		/// Activate to never show a reminder of an upcoming license expiration.
		/// </summary>
		public bool DisableLicenseExpirationReminder { get; set; } = false;
		
		/// <summary>
		/// Activate to disable printer tab. This disables the functionalities to add/delete/rename PDFCreator printers, change profile assignments and set the primary pdfcreator printer.
		/// </summary>
		public bool DisablePrinterTab { get; set; } = false;
		
		/// <summary>
		/// Activate to completely disable the editing of profiles.
		/// </summary>
		public bool DisableProfileManagement { get; set; } = false;
		
		/// <summary>
		/// Activate to disable the rss news feed.
		/// </summary>
		public bool DisableRssFeed { get; set; } = false;
		
		/// <summary>
		/// Disable tips on home screen.
		/// </summary>
		public bool DisableTips { get; set; } = false;
		
		/// <summary>
		/// Activate to disable title tab. This disables the functionality to change title replacements.
		/// </summary>
		public bool DisableTitleTab { get; set; } = false;
		
		/// <summary>
		/// Activate to completely disable the anonymous usage statistics.
		/// </summary>
		public bool DisableUsageStatistics { get; set; } = false;
		
		/// <summary>
		/// Activate to hide license tab.
		/// </summary>
		public bool HideLicenseTab { get; set; } = false;
		
		/// <summary>
		/// Activate to hide information about our customizable PDF Editor.
		/// </summary>
		public bool HidePdfArchitectInfo { get; set; } = false;
		
		/// <summary>
		/// Activate to load application settings from ini file in "%ProgramData%\pdfforge\PDFCreator". This is activated by default and must be explicitly disabled.
		/// </summary>
		public bool LoadSharedAppSettings { get; set; } = true;
		
		/// <summary>
		/// Activate to load profiles from ini file in "%ProgramData%\pdfforge\PDFCreator". This is activated by default and must be explicitly disabled.
		/// </summary>
		public bool LoadSharedProfiles { get; set; } = true;
		
		/// <summary>
		/// Set the filename of the shared settings file. The default is 'settings.ini'.
		/// </summary>
		public string SharedSettingsFilename { get; set; } = "settings";
		
		
		public void ReadValues(Data data, string path = "")
		{
			ApplicationSettings.ReadValues(data, path + @"ApplicationSettings\");
			CreatorAppSettings.ReadValues(data, path + @"CreatorAppSettings\");
			AllowUserDefinedProfiles = bool.TryParse(data.GetValue(@"" + path + @"AllowUserDefinedProfiles"), out var tmpAllowUserDefinedProfiles) ? tmpAllowUserDefinedProfiles : true;
			DisableAccountsTab = bool.TryParse(data.GetValue(@"" + path + @"DisableAccountsTab"), out var tmpDisableAccountsTab) ? tmpDisableAccountsTab : false;
			DisableApplicationSettings = bool.TryParse(data.GetValue(@"" + path + @"DisableApplicationSettings"), out var tmpDisableApplicationSettings) ? tmpDisableApplicationSettings : false;
			DisableDebugTab = bool.TryParse(data.GetValue(@"" + path + @"DisableDebugTab"), out var tmpDisableDebugTab) ? tmpDisableDebugTab : false;
			DisableHistory = bool.TryParse(data.GetValue(@"" + path + @"DisableHistory"), out var tmpDisableHistory) ? tmpDisableHistory : false;
			DisableLicenseExpirationReminder = bool.TryParse(data.GetValue(@"" + path + @"DisableLicenseExpirationReminder"), out var tmpDisableLicenseExpirationReminder) ? tmpDisableLicenseExpirationReminder : false;
			DisablePrinterTab = bool.TryParse(data.GetValue(@"" + path + @"DisablePrinterTab"), out var tmpDisablePrinterTab) ? tmpDisablePrinterTab : false;
			DisableProfileManagement = bool.TryParse(data.GetValue(@"" + path + @"DisableProfileManagement"), out var tmpDisableProfileManagement) ? tmpDisableProfileManagement : false;
			DisableRssFeed = bool.TryParse(data.GetValue(@"" + path + @"DisableRssFeed"), out var tmpDisableRssFeed) ? tmpDisableRssFeed : false;
			DisableTips = bool.TryParse(data.GetValue(@"" + path + @"DisableTips"), out var tmpDisableTips) ? tmpDisableTips : false;
			DisableTitleTab = bool.TryParse(data.GetValue(@"" + path + @"DisableTitleTab"), out var tmpDisableTitleTab) ? tmpDisableTitleTab : false;
			DisableUsageStatistics = bool.TryParse(data.GetValue(@"" + path + @"DisableUsageStatistics"), out var tmpDisableUsageStatistics) ? tmpDisableUsageStatistics : false;
			HideLicenseTab = bool.TryParse(data.GetValue(@"" + path + @"HideLicenseTab"), out var tmpHideLicenseTab) ? tmpHideLicenseTab : false;
			HidePdfArchitectInfo = bool.TryParse(data.GetValue(@"" + path + @"HidePdfArchitectInfo"), out var tmpHidePdfArchitectInfo) ? tmpHidePdfArchitectInfo : false;
			LoadSharedAppSettings = bool.TryParse(data.GetValue(@"" + path + @"LoadSharedAppSettings"), out var tmpLoadSharedAppSettings) ? tmpLoadSharedAppSettings : true;
			LoadSharedProfiles = bool.TryParse(data.GetValue(@"" + path + @"LoadSharedProfiles"), out var tmpLoadSharedProfiles) ? tmpLoadSharedProfiles : true;
			try { SharedSettingsFilename = Data.UnescapeString(data.GetValue(@"" + path + @"SharedSettingsFilename")); } catch { SharedSettingsFilename = "settings";}
		}
		
		public void StoreValues(Data data, string path)
		{
			ApplicationSettings.StoreValues(data, path + @"ApplicationSettings\");
			CreatorAppSettings.StoreValues(data, path + @"CreatorAppSettings\");
			data.SetValue(@"" + path + @"AllowUserDefinedProfiles", AllowUserDefinedProfiles.ToString());
			data.SetValue(@"" + path + @"DisableAccountsTab", DisableAccountsTab.ToString());
			data.SetValue(@"" + path + @"DisableApplicationSettings", DisableApplicationSettings.ToString());
			data.SetValue(@"" + path + @"DisableDebugTab", DisableDebugTab.ToString());
			data.SetValue(@"" + path + @"DisableHistory", DisableHistory.ToString());
			data.SetValue(@"" + path + @"DisableLicenseExpirationReminder", DisableLicenseExpirationReminder.ToString());
			data.SetValue(@"" + path + @"DisablePrinterTab", DisablePrinterTab.ToString());
			data.SetValue(@"" + path + @"DisableProfileManagement", DisableProfileManagement.ToString());
			data.SetValue(@"" + path + @"DisableRssFeed", DisableRssFeed.ToString());
			data.SetValue(@"" + path + @"DisableTips", DisableTips.ToString());
			data.SetValue(@"" + path + @"DisableTitleTab", DisableTitleTab.ToString());
			data.SetValue(@"" + path + @"DisableUsageStatistics", DisableUsageStatistics.ToString());
			data.SetValue(@"" + path + @"HideLicenseTab", HideLicenseTab.ToString());
			data.SetValue(@"" + path + @"HidePdfArchitectInfo", HidePdfArchitectInfo.ToString());
			data.SetValue(@"" + path + @"LoadSharedAppSettings", LoadSharedAppSettings.ToString());
			data.SetValue(@"" + path + @"LoadSharedProfiles", LoadSharedProfiles.ToString());
			data.SetValue(@"" + path + @"SharedSettingsFilename", Data.EscapeString(SharedSettingsFilename));
		}
		
		public GpoSettings Copy()
		{
			GpoSettings copy = new GpoSettings();
			
			copy.ApplicationSettings = ApplicationSettings.Copy();
			copy.CreatorAppSettings = CreatorAppSettings.Copy();
			copy.AllowUserDefinedProfiles = AllowUserDefinedProfiles;
			copy.DisableAccountsTab = DisableAccountsTab;
			copy.DisableApplicationSettings = DisableApplicationSettings;
			copy.DisableDebugTab = DisableDebugTab;
			copy.DisableHistory = DisableHistory;
			copy.DisableLicenseExpirationReminder = DisableLicenseExpirationReminder;
			copy.DisablePrinterTab = DisablePrinterTab;
			copy.DisableProfileManagement = DisableProfileManagement;
			copy.DisableRssFeed = DisableRssFeed;
			copy.DisableTips = DisableTips;
			copy.DisableTitleTab = DisableTitleTab;
			copy.DisableUsageStatistics = DisableUsageStatistics;
			copy.HideLicenseTab = HideLicenseTab;
			copy.HidePdfArchitectInfo = HidePdfArchitectInfo;
			copy.LoadSharedAppSettings = LoadSharedAppSettings;
			copy.LoadSharedProfiles = LoadSharedProfiles;
			copy.SharedSettingsFilename = SharedSettingsFilename;
			return copy;
		}
		
		public void ReplaceWith(GpoSettings source)
		{
			ApplicationSettings.ReplaceWith(source.ApplicationSettings);
			CreatorAppSettings.ReplaceWith(source.CreatorAppSettings);
			if(AllowUserDefinedProfiles != source.AllowUserDefinedProfiles)
				AllowUserDefinedProfiles = source.AllowUserDefinedProfiles;
				
			if(DisableAccountsTab != source.DisableAccountsTab)
				DisableAccountsTab = source.DisableAccountsTab;
				
			if(DisableApplicationSettings != source.DisableApplicationSettings)
				DisableApplicationSettings = source.DisableApplicationSettings;
				
			if(DisableDebugTab != source.DisableDebugTab)
				DisableDebugTab = source.DisableDebugTab;
				
			if(DisableHistory != source.DisableHistory)
				DisableHistory = source.DisableHistory;
				
			if(DisableLicenseExpirationReminder != source.DisableLicenseExpirationReminder)
				DisableLicenseExpirationReminder = source.DisableLicenseExpirationReminder;
				
			if(DisablePrinterTab != source.DisablePrinterTab)
				DisablePrinterTab = source.DisablePrinterTab;
				
			if(DisableProfileManagement != source.DisableProfileManagement)
				DisableProfileManagement = source.DisableProfileManagement;
				
			if(DisableRssFeed != source.DisableRssFeed)
				DisableRssFeed = source.DisableRssFeed;
				
			if(DisableTips != source.DisableTips)
				DisableTips = source.DisableTips;
				
			if(DisableTitleTab != source.DisableTitleTab)
				DisableTitleTab = source.DisableTitleTab;
				
			if(DisableUsageStatistics != source.DisableUsageStatistics)
				DisableUsageStatistics = source.DisableUsageStatistics;
				
			if(HideLicenseTab != source.HideLicenseTab)
				HideLicenseTab = source.HideLicenseTab;
				
			if(HidePdfArchitectInfo != source.HidePdfArchitectInfo)
				HidePdfArchitectInfo = source.HidePdfArchitectInfo;
				
			if(LoadSharedAppSettings != source.LoadSharedAppSettings)
				LoadSharedAppSettings = source.LoadSharedAppSettings;
				
			if(LoadSharedProfiles != source.LoadSharedProfiles)
				LoadSharedProfiles = source.LoadSharedProfiles;
				
			if(SharedSettingsFilename != source.SharedSettingsFilename)
				SharedSettingsFilename = source.SharedSettingsFilename;
				
		}
		
		public override bool Equals(object o)
		{
			if (!(o is GpoSettings)) return false;
			GpoSettings v = o as GpoSettings;
			
			if (!Object.Equals(ApplicationSettings, v.ApplicationSettings)) return false;
			if (!Object.Equals(CreatorAppSettings, v.CreatorAppSettings)) return false;
			if (!Object.Equals(AllowUserDefinedProfiles, v.AllowUserDefinedProfiles)) return false;
			if (!Object.Equals(DisableAccountsTab, v.DisableAccountsTab)) return false;
			if (!Object.Equals(DisableApplicationSettings, v.DisableApplicationSettings)) return false;
			if (!Object.Equals(DisableDebugTab, v.DisableDebugTab)) return false;
			if (!Object.Equals(DisableHistory, v.DisableHistory)) return false;
			if (!Object.Equals(DisableLicenseExpirationReminder, v.DisableLicenseExpirationReminder)) return false;
			if (!Object.Equals(DisablePrinterTab, v.DisablePrinterTab)) return false;
			if (!Object.Equals(DisableProfileManagement, v.DisableProfileManagement)) return false;
			if (!Object.Equals(DisableRssFeed, v.DisableRssFeed)) return false;
			if (!Object.Equals(DisableTips, v.DisableTips)) return false;
			if (!Object.Equals(DisableTitleTab, v.DisableTitleTab)) return false;
			if (!Object.Equals(DisableUsageStatistics, v.DisableUsageStatistics)) return false;
			if (!Object.Equals(HideLicenseTab, v.HideLicenseTab)) return false;
			if (!Object.Equals(HidePdfArchitectInfo, v.HidePdfArchitectInfo)) return false;
			if (!Object.Equals(LoadSharedAppSettings, v.LoadSharedAppSettings)) return false;
			if (!Object.Equals(LoadSharedProfiles, v.LoadSharedProfiles)) return false;
			if (!Object.Equals(SharedSettingsFilename, v.SharedSettingsFilename)) return false;
			return true;
		}
		
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}
		
	}
}
