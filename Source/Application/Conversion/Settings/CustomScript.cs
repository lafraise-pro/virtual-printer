using pdfforge.DataStorage.Storage;
using pdfforge.DataStorage;
using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System;

// ! This file is generated automatically.
// ! Do not edit it outside the sections for custom code.
// ! These changes will be deleted during the next generation run

namespace pdfforge.PDFCreator.Conversion.Settings
{
	/// <summary>
	/// Pre- and post-conversion actions calling functions from a custom script
	/// </summary>
	public partial class CustomScript : INotifyPropertyChanged {
		#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore 67
		
		
		/// <summary>
		/// Enables the custom script pre- and post-conversion action
		/// </summary>
		public bool Enabled { get; set; } = false;
		
		/// <summary>
		/// Filename of the custom script in application directory 'Cs-Scripts' folder
		/// </summary>
		public string ScriptFilename { get; set; } = "";
		
		
		public void ReadValues(Data data, string path = "")
		{
			Enabled = bool.TryParse(data.GetValue(@"" + path + @"Enabled"), out var tmpEnabled) ? tmpEnabled : false;
			try { ScriptFilename = Data.UnescapeString(data.GetValue(@"" + path + @"ScriptFilename")); } catch { ScriptFilename = "";}
		}
		
		public void StoreValues(Data data, string path)
		{
			data.SetValue(@"" + path + @"Enabled", Enabled.ToString());
			data.SetValue(@"" + path + @"ScriptFilename", Data.EscapeString(ScriptFilename));
		}
		
		public CustomScript Copy()
		{
			CustomScript copy = new CustomScript();
			
			copy.Enabled = Enabled;
			copy.ScriptFilename = ScriptFilename;
			return copy;
		}
		
		public void ReplaceWith(CustomScript source)
		{
			if(Enabled != source.Enabled)
				Enabled = source.Enabled;
				
			if(ScriptFilename != source.ScriptFilename)
				ScriptFilename = source.ScriptFilename;
				
		}
		
		public override bool Equals(object o)
		{
			if (!(o is CustomScript)) return false;
			CustomScript v = o as CustomScript;
			
			if (!Object.Equals(Enabled, v.Enabled)) return false;
			if (!Object.Equals(ScriptFilename, v.ScriptFilename)) return false;
			return true;
		}
		
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}
		
	}
}
