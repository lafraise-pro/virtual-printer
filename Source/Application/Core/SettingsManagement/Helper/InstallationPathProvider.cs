using System;
using Microsoft.Win32;
using pdfforge.PDFCreator.Core.SettingsManagementInterface;

namespace pdfforge.PDFCreator.Core.SettingsManagement.Helper
{
    public class InstallationPathProvider : IInstallationPathProvider
    {
        public InstallationPathProvider(string applicationRegistryPath, string settingsRegistryPath,
            string applicationGuid, RegistryHive registryHive)
        {
            SettingsRegistryPath = settingsRegistryPath;
            ApplicationGuid = applicationGuid;
            ApplicationRegistryPath = applicationRegistryPath;
            RegistryHive = GetHiveString(registryHive);
        }

        private string GetHiveString(RegistryHive registryHive)
        {
            switch (registryHive)
            {
                case Microsoft.Win32.RegistryHive.CurrentUser: return "HKEY_CURRENT_USER";
                case Microsoft.Win32.RegistryHive.LocalMachine: return "HKEY_LOCAL_MACHINE";
            }

            throw new ArgumentOutOfRangeException(nameof(registryHive), $"The registry hive {registryHive} is not supported!");
        }

        public string SettingsRegistryPath { get; }
        public string ApplicationRegistryPath { get; }
        public string ApplicationGuid { get; }
        public string RegistryHive { get; }
    }

    public static class InstallationPathProviders
    {
        private static string _pdfcreatorRegPath = @"Software\lafraise\LaFraiseVirtualPrinter";
        private static string _pdfcreatorServerRegPath = @"Software\lafraise\LaFraiseVirtualPrinter Server";
        private static string _pdfcreatorProductId = "{8E9BCD51-04FC-4712-B33A-C4B62AC7AB8D}";

        public static InstallationPathProvider PDFCreatorProvider => new InstallationPathProvider(_pdfcreatorRegPath, _pdfcreatorRegPath + @"\Settings", _pdfcreatorProductId, RegistryHive.CurrentUser);
        public static InstallationPathProvider PDFCreatorServerProvider => new InstallationPathProvider(_pdfcreatorServerRegPath, _pdfcreatorServerRegPath + @"\Settings", _pdfcreatorProductId, RegistryHive.LocalMachine);
    }
}
