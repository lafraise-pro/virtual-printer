﻿using pdfforge.PDFCreator.Core.Controller;
using pdfforge.PDFCreator.UI.Presentation.Events;
using pdfforge.PDFCreator.Utilities.Threading;
using Prism.Events;

namespace pdfforge.PDFCreator.UI.Presentation
{
    public class MainShellLauncher : IMainWindowThreadLauncher
    {
        private readonly IShellManager _shellManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICurrentSettingsProvider _currentSettingsProvider;
        private readonly IThreadManager _threadManager;
        private ISynchronizedThread _mainWindowThread;

        public MainShellLauncher(IThreadManager threadManager, IShellManager shellManager, IEventAggregator eventAggregator, ICurrentSettingsProvider currentSettingsProvider)
        {
            _threadManager = threadManager;
            _shellManager = shellManager;
            _eventAggregator = eventAggregator;
            _currentSettingsProvider = currentSettingsProvider;
        }

        public void LaunchMainWindow()
        {
            if (_mainWindowThread != null)
            {
                _shellManager.MainShellToFront();
                return;
            }
            _mainWindowThread = _threadManager.StartSynchronizedUiThread(MainWindowLaunchThreadMethod, "MainWindowThread");
        }

        private void MainWindowLaunchThreadMethod()
        {
            try
            {
                _currentSettingsProvider.Reset(true);
                _shellManager.ShowMainShell();
            }
            finally
            {
                _mainWindowThread = null;
                _eventAggregator.GetEvent<MainShellClosedEvent>().Publish();
            }
        }

        public void SwitchPrintJobShellToMergeWindow()
        {
            _eventAggregator.GetEvent<ManagePrintJobEvent>().Publish();
        }

        public bool IsPrintJobShellOpen()
        {
            return _shellManager.PrintJobShellIsOpen;
        }
    }
}
