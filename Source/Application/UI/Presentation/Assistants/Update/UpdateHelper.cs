using Microsoft.Win32;
using NLog;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using pdfforge.PDFCreator.Conversion.Settings.Extensions;
using pdfforge.PDFCreator.Conversion.Settings.GroupPolicies;
using pdfforge.PDFCreator.Core.Services.Update;
using pdfforge.PDFCreator.UI.Presentation.Helper.Version;
using pdfforge.PDFCreator.Utilities;
using Prism.Events;
using System;
using System.Threading.Tasks;
using pdfforge.PDFCreator.Core.SettingsManagement.Helper;
using pdfforge.PDFCreator.Core.SettingsManagementInterface;

namespace pdfforge.PDFCreator.UI.Presentation.Assistants.Update
{
    public class UpdateHelper : IUpdateHelper
    {
        private readonly IGpoSettings _gpoSettings;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOnlineVersionHelper _onlineVersionHelper;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICurrentSettings<ApplicationSettings> _settingsProvider;

        private readonly string _skipVersionRegistryPath;
        private readonly IVersionHelper _versionHelper;
        private bool _showUpdateDuringSession = false;
        private bool _isTimeForNextUpdate = false;

        public UpdateHelper(ICurrentSettings<ApplicationSettings> applicationSettingsProvider,
            IVersionHelper versionHelper, IInstallationPathProvider installationPathProvider, IGpoSettings gpoSettings,
            IEventAggregator eventAggregator, IOnlineVersionHelper onlineVersionHelper)
        {
            _settingsProvider = applicationSettingsProvider;
            _versionHelper = versionHelper;
            _gpoSettings = gpoSettings;
            _eventAggregator = eventAggregator;
            _onlineVersionHelper = onlineVersionHelper;
            _skipVersionRegistryPath = installationPathProvider.RegistryHive + "\\" + installationPathProvider.ApplicationRegistryPath;
        }

        /// <summary>
        ///     Current time for the next automated updateCheck
        /// </summary>
        private DateTime NextUpdate
        {
            get
            {
                if (_settingsProvider.Settings != null)
                {
                    return _settingsProvider.Settings.NextUpdate;
                }

                return DateTime.Now;
            }
            set => _settingsProvider.Settings.NextUpdate = value;
        }

        private UpdateInterval UpdateInterval
        {
            get
            {
                if (string.IsNullOrEmpty(_gpoSettings?.UpdateInterval))
                    return _settingsProvider.Settings.UpdateInterval;
                return UpdateIntervalHelper.ParseUpdateInterval(_gpoSettings.UpdateInterval);
            }
        }

        /// <summary>
        ///     Flag to report, if UpdateProcedure(Thread) is already running
        /// </summary>
        public bool UpdateProcedureIsRunning { get; private set; }

        public bool UpdatesEnabled => true;

        public async Task UpdateCheckAsync(bool checkNecessity)
        {
            UpdateProcedureIsRunning = false;
        }

        public bool IsTimeForNextUpdate()
        {
            _isTimeForNextUpdate = false;
            return false;
        }

        public bool UpdateShouldBeShown()
        {
            _showUpdateDuringSession = false;
            return false;
        }

        public void ShowLater()
        {
            SetNewUpdateTime();

            _isTimeForNextUpdate = false;
            _eventAggregator.GetEvent<SetShowUpdateEvent>().Publish(false);
        }

        private async Task Update(UpdateEventArgs eventArgs, bool checkNecessity)
        {
            return;
        }

        private bool VersionShouldNotBeSkipped(bool checkNecessity, Version onlineVersion)
        {
            if (!checkNecessity)
                return true;

            var skipVersion = ReadSkipVersionFromRegistry();
            return skipVersion.CompareTo(onlineVersion) < 0;
        }

        private Version ReadSkipVersionFromRegistry()
        {
            var versionString = Registry.GetValue(_skipVersionRegistryPath, "SkipVersion", "")?.ToString();
            if (string.IsNullOrWhiteSpace(versionString))
                return new Version(0, 0);

            var success = Version.TryParse(versionString, out var version);

            return success ? version : new Version(0, 0);
        }

        private void SetSkipVersionInRegistry(Version onlineVersion)
        {
            Registry.SetValue(_skipVersionRegistryPath, "SkipVersion", onlineVersion.ToString());
        }

        public void SetNewUpdateTime()
        {
            var timeSpan = UpdateInterval.ToTimeSpan();
            NextUpdate = DateTime.Now.Add(timeSpan);
            _showUpdateDuringSession = false;
        }

        public void SkipVersion()
        {
            var onlineVersion = _onlineVersionHelper.GetOnlineVersion();

            if (onlineVersion != null)
                SetSkipVersionInRegistry(onlineVersion.Version);

            SetNewUpdateTime();
            _isTimeForNextUpdate = false;
            _eventAggregator.GetEvent<SetShowUpdateEvent>().Publish(false);
        }

        public async Task<bool> IsUpdateAvailableAsync(bool checkNecessity)
        {
            return false;
        }

        public bool IsUpdateAvailable()
        {
            return false;
        }
    }
}
