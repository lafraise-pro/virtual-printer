﻿using NLog;
using pdfforge.PDFCreator.Core.DirectConversion;
using pdfforge.PDFCreator.Core.Startup.AppStarts;
using pdfforge.PDFCreator.Core.StartupInterface;
using pdfforge.PDFCreator.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.IO;

namespace pdfforge.PDFCreator.Core.Startup
{
    public class AppStartFactory
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IAppStartResolver _appStartResolver;
        private readonly IPathUtil _pathUtil;
        private readonly IDirectConversionHelper _directConversionHelper;

        private static readonly string[] DeprecatedParameters = { "InitializeSettings", "RestorePrinters" };

        public AppStartFactory(IAppStartResolver appStartResolver,
            IPathUtil pathUtil,
            IDirectConversionHelper directConversionHelper)
        {
            _appStartResolver = appStartResolver;
            _pathUtil = pathUtil;
            _directConversionHelper = directConversionHelper;
        }

        public IAppStart CreateApplicationStart(string[] commandLineArgs)
        {
            LogCommandLineParameters(commandLineArgs);

            if (IsDragAndDropStart(commandLineArgs))
            {
                _logger.Debug("Launched Drag & Drop");
                var dragAndDropStart = _appStartResolver.ResolveAppStart<DragAndDropStart>();
                dragAndDropStart.DroppedFiles = commandLineArgs.ToList();
                return dragAndDropStart;
            }

            var commandLineParser = new CommandLineParser(commandLineArgs);

            foreach (var parameter in DeprecatedParameters)
            {
                if (commandLineParser.HasArgument(parameter))
                    throw new DeprecatedParameterException(parameter);
            }

            var appStartParameters = new AppStartParameters();
            appStartParameters.ManagePrintJobs = commandLineParser.HasArgument("ManagePrintJobs");

            if (commandLineParser.HasArgumentWithValue("InfoDataFile"))
            {
                var infFile = commandLineParser.GetArgument("InfoDataFile");
                var newPrintJobStart = _appStartResolver.ResolveAppStart<NewPrintJobStart>();
                newPrintJobStart.NewJobInfoFile = infFile;
                newPrintJobStart.AppStartParameters = appStartParameters;
                return newPrintJobStart;
            }

            appStartParameters.Silent = commandLineParser.HasArgument("Silent");
            appStartParameters.Silent |= commandLineParser.HasArgument("s");
            appStartParameters.Merge = commandLineParser.HasArgument("Merge");
            appStartParameters.Merge |= commandLineParser.HasArgument("m");
            appStartParameters.Printer = FindPrinterName(commandLineParser);
            appStartParameters.Profile = FindProfileParameter(commandLineParser);
            appStartParameters.OutputFile = FindOutputfileParameter(commandLineParser);

            var files = new List<string>();
            if (commandLineParser.HasArgumentWithValue("PrintFile"))
            {
                var printFile = commandLineParser.GetArgument("PrintFile");
                if (_directConversionHelper.CanConvertDirectly(printFile))
                    files.Add(printFile); //Add file and proceed with directConversion see below
                else
                {
                    var printFileStart = _appStartResolver.ResolveAppStart<PrintFileStart>();
                    printFileStart.PrintFile = printFile;
                    printFileStart.AppStartParameters = appStartParameters;
                    return printFileStart;
                }
            }

            AddDirectConversionFiles(files, commandLineParser);
            var parameterlessFiles = commandLineArgs.Where(IsFileArg).ToList();
            files.AddRange(parameterlessFiles);

            if (appStartParameters.Merge)
            {
                _logger.Debug("Launched MergeAppStart");
                var mergeAppStart = _appStartResolver.ResolveAppStart<MergeAppStart>();
                mergeAppStart.FilesForMerge = files;
                mergeAppStart.AppStartParameters = appStartParameters;
                return mergeAppStart;
            }

            if (files.Count > 0)
            {
                _logger.Debug("Launched DragAndDropStart");
                var dragAndDropStart = _appStartResolver.ResolveAppStart<DragAndDropStart>();
                dragAndDropStart.DroppedFiles = files;
                dragAndDropStart.AppStartParameters = appStartParameters;
                return dragAndDropStart;
            }

            // ... else we have a MainWindowStart
            var mainWindowStart = _appStartResolver.ResolveAppStart<MainWindowStart>();
            // suppress ManagePrintJobWindows together with the MainWindow (at this point no conversion was requested)
            appStartParameters.ManagePrintJobs = false;
            mainWindowStart.AppStartParameters = appStartParameters;

            return mainWindowStart;
        }

        private void LogCommandLineParameters(string[] commandLineArguments)
        {
            if (commandLineArguments.Length > 0)
            {
                _logger.Info("Command Line parameters: \r\n" + string.Join(" ", commandLineArguments));
            }
        }

        private bool IsCommandArg(string arg) => arg.StartsWith("/") || arg.StartsWith("-");

        private bool IsFileArg(string arg) => !IsCommandArg(arg) && _pathUtil.IsValidRootedPath(arg);

        private bool IsDragAndDropStart(string[] args)
        {
            if (args == null)
                return false;

            if (!args.Any())
                return false;

            if (args.Any(IsCommandArg))
                return false;

            return args.All(IsFileArg);
        }

        private void AddDirectConversionFiles(List<string> files, CommandLineParser commandLineParser)
        {
            if (commandLineParser.HasArgumentWithValue("PdfFile"))
            {
                var pdfFile = commandLineParser.GetArgument("PdfFile");
                if (!string.IsNullOrWhiteSpace(pdfFile))
                    files.Add(pdfFile);
            }

            if (commandLineParser.HasArgumentWithValue("PsFile"))
            {
                var psFile = commandLineParser.GetArgument("PsFile");
                if (!string.IsNullOrWhiteSpace(psFile))
                    files.Add(psFile);
            }
        }

        private string FindPrinterName(CommandLineParser commandLineParser)
        {
            if (commandLineParser.HasArgumentWithValue("PrinterName"))
                return commandLineParser.GetArgument("PrinterName");
            if (commandLineParser.HasArgumentWithValue("Printer"))
                return commandLineParser.GetArgument("Printer");
            return "";
        }

        private string FindOutputfileParameter(CommandLineParser commandLineParser)
        {
            if (commandLineParser.HasArgument("Outputfile"))
            {
                var outputFile = commandLineParser.GetArgument("Outputfile");
                var directoryName = PathSafe.GetDirectoryName(outputFile);
                var fileNameWithoutExtension = PathSafe.GetFileNameWithoutExtension(outputFile);
                return PathSafe.Combine(directoryName, fileNameWithoutExtension);
            }

            return "";
        }

        private string FindProfileParameter(CommandLineParser commandLineParser)
        {
            if (commandLineParser.HasArgument("Profile"))
                return commandLineParser.GetArgument("Profile");

            return "";
        }
    }

    public class DeprecatedParameterException : Exception
    {
        public string ParameterName { get; }

        public DeprecatedParameterException(string parameterName)
        : base($"The parameter {parameterName} is not supported anymore!")
        {
            ParameterName = parameterName;
        }
    }
}
