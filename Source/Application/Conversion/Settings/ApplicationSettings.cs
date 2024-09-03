using pdfforge.DataStorage.Storage;
using pdfforge.DataStorage;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System;

// ! This file is generated automatically.
// ! Do not edit it outside the sections for custom code.
// ! These changes will be deleted during the next generation run

namespace pdfforge.PDFCreator.Conversion.Settings
{
	/// <summary>
	/// PDFCreator application settings
	/// </summary>
	public partial class ApplicationSettings : INotifyPropertyChanged {
		#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore 67
		
		
		public Accounts Accounts { get; set; } = new Accounts();
		
		public JobHistory JobHistory { get; set; } = new JobHistory();
		
		public ObservableCollection<PrinterMapping> PrinterMappings { get; set; } = new ObservableCollection<PrinterMapping>();
		public RssFeed RssFeed { get; set; } = new RssFeed();
		
		public ObservableCollection<TitleReplacement> TitleReplacement { get; set; } = new ObservableCollection<TitleReplacement>();
		public UsageStatistics UsageStatistics { get; set; } = new UsageStatistics();
		
		public int ConversionTimeout { get; set; } = 60;
		
		public bool EnableTips { get; set; } = true;
		
		public string Language { get; set; } = "";
		
		/// <summary>
		/// Remind that license will expire soon
		/// </summary>
		public DateTime LicenseExpirationReminder { get; set; } = DateTime.Now;
		
		public LoggingLevel LoggingLevel { get; set; } = LoggingLevel.Error;
		
		public DateTime NextUpdate { get; set; } = DateTime.Now;
		
		/// <summary>
		/// Sets page orientation.
		/// </summary>
		public PageOrientation PageOrientation { get; set; } = PageOrientation.Automatic;
		
		/// <summary>
		/// et the page size.
		/// </summary>
		public PageSize PageSize { get; set; } = PageSize.Automatic;
		
		/// <summary>
		/// Set the filename of the shared settings file. The default is 'settings.ini'.
		/// </summary>
		public string SharedSettingsFilename { get; set; } = "settings";
		
		/// <summary>
		/// Defines the unit of measurement for the signature position.
		/// </summary>
		public UnitOfMeasurement UnitOfMeasurement { get; set; } = UnitOfMeasurement.Centimeter;
		
		public UpdateInterval UpdateInterval { get; set; } = UpdateInterval.Weekly;
		
		
		public void ReadValues(Data data, string path = "")
		{
			Accounts.ReadValues(data, path + @"Accounts\");
			JobHistory.ReadValues(data, path + @"JobHistory\");
			
			try
			{
				int numClasses = int.Parse(data.GetValue(@"" + path + @"PrinterMappings\numClasses"));
				for (int i = 0; i < numClasses; i++)
				{
					PrinterMapping tmp = new PrinterMapping();
					tmp.ReadValues(data, @"" + path + @"PrinterMappings\" + i + @"\");
					PrinterMappings.Add(tmp);
				}
			} catch {}
			
			RssFeed.ReadValues(data, path + @"RssFeed\");
			
			try
			{
				int numClasses = int.Parse(data.GetValue(@"" + path + @"TitleReplacement\numClasses"));
				for (int i = 0; i < numClasses; i++)
				{
					TitleReplacement tmp = new TitleReplacement();
					tmp.ReadValues(data, @"" + path + @"TitleReplacement\" + i + @"\");
					TitleReplacement.Add(tmp);
				}
			} catch {}
			
			UsageStatistics.ReadValues(data, path + @"UsageStatistics\");
			ConversionTimeout = int.TryParse(data.GetValue(@"" + path + @"ConversionTimeout"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tmpConversionTimeout) ? tmpConversionTimeout : 60;
			EnableTips = bool.TryParse(data.GetValue(@"" + path + @"EnableTips"), out var tmpEnableTips) ? tmpEnableTips : true;
			try { Language = Data.UnescapeString(data.GetValue(@"" + path + @"Language")); } catch { Language = "";}
			LicenseExpirationReminder = DateTime.TryParse(data.GetValue(@"" + path + @"LicenseExpirationReminder"), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var tmpLicenseExpirationReminder) ? tmpLicenseExpirationReminder : DateTime.Now;
			LoggingLevel = Enum.TryParse<LoggingLevel>(data.GetValue(@"" + path + @"LoggingLevel"), out var tmpLoggingLevel) ? tmpLoggingLevel : LoggingLevel.Error;
			NextUpdate = DateTime.TryParse(data.GetValue(@"" + path + @"NextUpdate"), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var tmpNextUpdate) ? tmpNextUpdate : DateTime.Now;
			PageOrientation = Enum.TryParse<PageOrientation>(data.GetValue(@"" + path + @"PageOrientation"), out var tmpPageOrientation) ? tmpPageOrientation : PageOrientation.Automatic;
			PageSize = Enum.TryParse<PageSize>(data.GetValue(@"" + path + @"PageSize"), out var tmpPageSize) ? tmpPageSize : PageSize.Automatic;
			try { SharedSettingsFilename = Data.UnescapeString(data.GetValue(@"" + path + @"SharedSettingsFilename")); } catch { SharedSettingsFilename = "settings";}
			UnitOfMeasurement = Enum.TryParse<UnitOfMeasurement>(data.GetValue(@"" + path + @"UnitOfMeasurement"), out var tmpUnitOfMeasurement) ? tmpUnitOfMeasurement : UnitOfMeasurement.Centimeter;
			UpdateInterval = Enum.TryParse<UpdateInterval>(data.GetValue(@"" + path + @"UpdateInterval"), out var tmpUpdateInterval) ? tmpUpdateInterval : UpdateInterval.Weekly;
		}
		
		public void StoreValues(Data data, string path)
		{
			Accounts.StoreValues(data, path + @"Accounts\");
			JobHistory.StoreValues(data, path + @"JobHistory\");
			
			for (int i = 0; i < PrinterMappings.Count; i++)
			{
				PrinterMapping tmp = PrinterMappings[i];
				tmp.StoreValues(data, @"" + path + @"PrinterMappings\" + i + @"\");
			}
			data.SetValue(@"" + path + @"PrinterMappings\numClasses", PrinterMappings.Count.ToString());
			
			RssFeed.StoreValues(data, path + @"RssFeed\");
			
			for (int i = 0; i < TitleReplacement.Count; i++)
			{
				TitleReplacement tmp = TitleReplacement[i];
				tmp.StoreValues(data, @"" + path + @"TitleReplacement\" + i + @"\");
			}
			data.SetValue(@"" + path + @"TitleReplacement\numClasses", TitleReplacement.Count.ToString());
			
			UsageStatistics.StoreValues(data, path + @"UsageStatistics\");
			data.SetValue(@"" + path + @"ConversionTimeout", ConversionTimeout.ToString(System.Globalization.CultureInfo.InvariantCulture));
			data.SetValue(@"" + path + @"EnableTips", EnableTips.ToString());
			data.SetValue(@"" + path + @"Language", Data.EscapeString(Language));
			data.SetValue(@"" + path + @"LicenseExpirationReminder", LicenseExpirationReminder.ToString("yyyy-MM-dd HH:mm:ss"));
			data.SetValue(@"" + path + @"LoggingLevel", LoggingLevel.ToString());
			data.SetValue(@"" + path + @"NextUpdate", NextUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
			data.SetValue(@"" + path + @"PageOrientation", PageOrientation.ToString());
			data.SetValue(@"" + path + @"PageSize", PageSize.ToString());
			data.SetValue(@"" + path + @"SharedSettingsFilename", Data.EscapeString(SharedSettingsFilename));
			data.SetValue(@"" + path + @"UnitOfMeasurement", UnitOfMeasurement.ToString());
			data.SetValue(@"" + path + @"UpdateInterval", UpdateInterval.ToString());
		}
		
		public ApplicationSettings Copy()
		{
			ApplicationSettings copy = new ApplicationSettings();
			
			copy.Accounts = Accounts.Copy();
			copy.JobHistory = JobHistory.Copy();
			
			copy.PrinterMappings = new ObservableCollection<PrinterMapping>();
			for (int i = 0; i < PrinterMappings.Count; i++)
			{
				copy.PrinterMappings.Add(PrinterMappings[i].Copy());
			}
			
			copy.RssFeed = RssFeed.Copy();
			
			copy.TitleReplacement = new ObservableCollection<TitleReplacement>();
			for (int i = 0; i < TitleReplacement.Count; i++)
			{
				copy.TitleReplacement.Add(TitleReplacement[i].Copy());
			}
			
			copy.UsageStatistics = UsageStatistics.Copy();
			copy.ConversionTimeout = ConversionTimeout;
			copy.EnableTips = EnableTips;
			copy.Language = Language;
			copy.LicenseExpirationReminder = LicenseExpirationReminder;
			copy.LoggingLevel = LoggingLevel;
			copy.NextUpdate = NextUpdate;
			copy.PageOrientation = PageOrientation;
			copy.PageSize = PageSize;
			copy.SharedSettingsFilename = SharedSettingsFilename;
			copy.UnitOfMeasurement = UnitOfMeasurement;
			copy.UpdateInterval = UpdateInterval;
			return copy;
		}
		
		public void ReplaceWith(ApplicationSettings source)
		{
			Accounts.ReplaceWith(source.Accounts);
			JobHistory.ReplaceWith(source.JobHistory);
			
			PrinterMappings.Clear();
			for (int i = 0; i < source.PrinterMappings.Count; i++)
			{
				PrinterMappings.Add(source.PrinterMappings[i].Copy());
			}
			
			RssFeed.ReplaceWith(source.RssFeed);
			
			TitleReplacement.Clear();
			for (int i = 0; i < source.TitleReplacement.Count; i++)
			{
				TitleReplacement.Add(source.TitleReplacement[i].Copy());
			}
			
			UsageStatistics.ReplaceWith(source.UsageStatistics);
			if(ConversionTimeout != source.ConversionTimeout)
				ConversionTimeout = source.ConversionTimeout;
				
			if(EnableTips != source.EnableTips)
				EnableTips = source.EnableTips;
				
			if(Language != source.Language)
				Language = source.Language;
				
			if(LicenseExpirationReminder != source.LicenseExpirationReminder)
				LicenseExpirationReminder = source.LicenseExpirationReminder;
				
			if(LoggingLevel != source.LoggingLevel)
				LoggingLevel = source.LoggingLevel;
				
			if(NextUpdate != source.NextUpdate)
				NextUpdate = source.NextUpdate;
				
			if(PageOrientation != source.PageOrientation)
				PageOrientation = source.PageOrientation;
				
			if(PageSize != source.PageSize)
				PageSize = source.PageSize;
				
			if(SharedSettingsFilename != source.SharedSettingsFilename)
				SharedSettingsFilename = source.SharedSettingsFilename;
				
			if(UnitOfMeasurement != source.UnitOfMeasurement)
				UnitOfMeasurement = source.UnitOfMeasurement;
				
			if(UpdateInterval != source.UpdateInterval)
				UpdateInterval = source.UpdateInterval;
				
		}
		
		public override bool Equals(object o)
		{
			if (!(o is ApplicationSettings)) return false;
			ApplicationSettings v = o as ApplicationSettings;
			
			if (!Object.Equals(Accounts, v.Accounts)) return false;
			if (!Object.Equals(JobHistory, v.JobHistory)) return false;
			
			if (PrinterMappings?.Count != v.PrinterMappings?.Count) return false;
			if (Object.ReferenceEquals(PrinterMappings, v.PrinterMappings)) return true;
			
			for (int i = 0; i < PrinterMappings.Count; i++)
			{
				if (!PrinterMappings[i].Equals(v.PrinterMappings[i])) return false;
			}
			
			if (!Object.Equals(RssFeed, v.RssFeed)) return false;
			
			if (TitleReplacement?.Count != v.TitleReplacement?.Count) return false;
			if (Object.ReferenceEquals(TitleReplacement, v.TitleReplacement)) return true;
			
			for (int i = 0; i < TitleReplacement.Count; i++)
			{
				if (!TitleReplacement[i].Equals(v.TitleReplacement[i])) return false;
			}
			
			if (!Object.Equals(UsageStatistics, v.UsageStatistics)) return false;
			if (!Object.Equals(ConversionTimeout, v.ConversionTimeout)) return false;
			if (!Object.Equals(EnableTips, v.EnableTips)) return false;
			if (!Object.Equals(Language, v.Language)) return false;
			if (!Object.Equals(LicenseExpirationReminder, v.LicenseExpirationReminder)) return false;
			if (!Object.Equals(LoggingLevel, v.LoggingLevel)) return false;
			if (!Object.Equals(NextUpdate, v.NextUpdate)) return false;
			if (!Object.Equals(PageOrientation, v.PageOrientation)) return false;
			if (!Object.Equals(PageSize, v.PageSize)) return false;
			if (!Object.Equals(SharedSettingsFilename, v.SharedSettingsFilename)) return false;
			if (!Object.Equals(UnitOfMeasurement, v.UnitOfMeasurement)) return false;
			if (!Object.Equals(UpdateInterval, v.UpdateInterval)) return false;
			return true;
		}
		
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}
		
	}
}
