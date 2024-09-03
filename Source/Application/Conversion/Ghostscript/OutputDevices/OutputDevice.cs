﻿using NLog;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using pdfforge.PDFCreator.Utilities;
using pdfforge.PDFCreator.Utilities.Tokens;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Text;
using SystemInterface.IO;
using SystemWrapper.IO;

namespace pdfforge.PDFCreator.Conversion.Ghostscript.OutputDevices
{
    /// <summary>
    ///     The abstract class OutputDevice holds methods and properties that handle the Ghostscript parameters. The device
    ///     independent elements are defined here.
    ///     Other classes inherit OutputDevice to extend the functionality with device-specific functionality, i.e. to create
    ///     PDF or PNG files.
    ///     Especially the abstract function AddDeviceSpecificParameters has to be implemented to add parameters that are
    ///     required to use a given device.
    /// </summary>
    public abstract class OutputDevice
    {
        private readonly ICommandLineUtil _commandLineUtil;
        private readonly IFormatProvider _numberFormat = CultureInfo.InvariantCulture.NumberFormat;

        protected readonly IFile FileWrap;
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOsHelper _osHelper;

        /// <summary>
        ///     A list of Distiller dictionary strings. They will be added after all parameters are set.
        /// </summary>
        protected IList<string> DistillerDictonaries = new List<string>();

        /// <summary>
        ///     A list of output files produced during the conversion
        /// </summary>
        public IList<string> TempOutputFiles = new List<string>();

        protected OutputDevice(Job job) : this(job, new FileWrap(), new OsHelper(), new CommandLineUtil())
        {
        }

        protected OutputDevice(Job job, IFile file, IOsHelper osHelper, ICommandLineUtil commandLineUtil)
        {
            Job = job;
            FileWrap = file;
            _osHelper = osHelper;
            _commandLineUtil = commandLineUtil;
        }

        /// <summary>
        ///     The Job that is converted
        /// </summary>
        public Job Job { get; }

        /// <summary>
        ///     Get the list of Ghostscript Parameters. This List contains of a basic set of parameters together with some
        ///     device-specific
        ///     parameters that will be added by the device implementation
        /// </summary>
        /// <param name="ghostscriptVersion"></param>
        /// <returns>A list of parameters that will be passed to Ghostscript</returns>
        public IList<string> GetGhostScriptParameters(GhostscriptVersion ghostscriptVersion)
        {
            IList<string> parameters = new List<string>();

            var outputFormatHelper = new OutputFormatHelper();

            parameters.Add("gs");
            parameters.Add("-I" + ghostscriptVersion.LibPaths);
            parameters.Add("-sFONTPATH=" + _osHelper.WindowsFontsFolder);

            parameters.Add("-dNOPAUSE");
            parameters.Add("-dBATCH");

            if (!outputFormatHelper.HasValidExtension(Job.OutputFileTemplate, Job.Profile.OutputFormat))
                outputFormatHelper.EnsureValidExtension(Job.OutputFileTemplate, Job.Profile.OutputFormat);

            AddOutputfileParameter(parameters);

            AddDeviceSpecificParameters(parameters);

            // Add user-defined parameters
            if (!string.IsNullOrEmpty(Job.Profile.Ghostscript.AdditionalGsParameters))
            {
                var args = _commandLineUtil.CommandLineToArgs(Job.Profile.Ghostscript.AdditionalGsParameters);
                foreach (var s in args)
                    parameters.Add(s);
            }

            //Dictonary-Parameters must be the last Parameters
            if (DistillerDictonaries.Count > 0)
            {
                parameters.Add("-c");
                foreach (var parameter in DistillerDictonaries)
                {
                    parameters.Add(parameter);
                }
            }

            //Don't add further paramters here, since the distiller-parameters should be the last!

            parameters.Add("-f");

            if (Job.Profile.Stamping.Enabled)
            {
                // Compose name of the stamp file based on the location and name of the inf file
                var stampFileName = PathSafe.Combine(Job.JobTempFolder,
                    PathSafe.GetFileNameWithoutExtension(Job.JobInfo.InfFile) + ".stm");
                CreateStampFile(stampFileName, Job.Profile, Job.TokenReplacer);
                parameters.Add(stampFileName);
            }

            SetCover(Job, parameters);

            foreach (var sfi in Job.JobInfo.SourceFiles)
            {
                parameters.Add(PathHelper.GetShortPathName(sfi.Filename));
            }

            SetAttachment(Job, parameters);

            // Compose name of the pdfmark file based on the location and name of the inf file
            var pdfMarkFileName = PathSafe.Combine(Job.JobTempFolder, "metadata.mtd");
            CreatePdfMarksFile(pdfMarkFileName);

            // Add pdfmark file as input file to set metadata
            parameters.Add(pdfMarkFileName);

            return parameters;
        }

        private void SetCover(Job job, IList<string> parameters)
        {
            if (Job.Profile.CoverPage.Enabled)
            {
                var coverFile = job.TokenReplacer.ReplaceTokens(Job.Profile.CoverPage.File);

                if (!FileWrap.Exists(coverFile))
                    return; // todo: Inform user. Probably way sooner.

                coverFile = PathHelper.GetShortPathName(coverFile);
                parameters.Add(PathHelper.GetShortPathName(coverFile));
            }
        }

        private void SetAttachment(Job job, IList<string> parameters)
        {
            if (Job.Profile.AttachmentPage.Enabled)
            {
                var attachmentFile = job.TokenReplacer.ReplaceTokens(Job.Profile.AttachmentPage.File);

                if (!FileWrap.Exists(attachmentFile))
                    return; // todo: Inform user. Probably way sooner.

                attachmentFile = PathHelper.GetShortPathName(attachmentFile);
                parameters.Add(PathHelper.GetShortPathName(attachmentFile));
            }
        }

        protected virtual void AddOutputfileParameter(IList<string> parameters)
        {
            parameters.Add("-sOutputFile=" + PathSafe.Combine(PathHelper.GetShortPathName(Job.JobTempOutputFolder), ComposeOutputFilename()));
        }

        /// <summary>
        ///     Create a file with metadata in the pdfmarks format. This file can be passed to Ghostscript to set Metadata of the
        ///     resulting document
        /// </summary>
        /// <param name="filename">Full path and filename of the resulting file</param>
        private void CreatePdfMarksFile(string filename)
        {
            var metadataContent = new StringBuilder();
            metadataContent.Append("/pdfmark where {pop} {userdict /pdfmark /cleartomark load put} ifelse\n[ ");
            metadataContent.Append("\n/Title " + EncodeGhostscriptParametersHex(Job.JobInfo.Metadata.Title));
            metadataContent.Append("\n/Author " + EncodeGhostscriptParametersHex(Job.JobInfo.Metadata.Author));
            metadataContent.Append("\n/Subject " + EncodeGhostscriptParametersHex(Job.JobInfo.Metadata.Subject));
            metadataContent.Append("\n/Keywords " + EncodeGhostscriptParametersHex(Job.JobInfo.Metadata.Keywords));
            metadataContent.Append("\n/Creator " + EncodeGhostscriptParametersHex(Job.Producer));
            metadataContent.Append("\n/Producer " + EncodeGhostscriptParametersHex(Job.Producer));
            metadataContent.Append("\n/DOCINFO pdfmark");

            AddViewerSettingsToMetadataContent(metadataContent);

            FileWrap.WriteAllText(filename, metadataContent.ToString());

            Logger.Debug("Created metadata file \"" + filename + "\"");
        }

        private string RgbToCmykColorString(Color color)
        {
            var red = color.R / 255.0;
            var green = color.G / 255.0;
            var blue = color.B / 255.0;

            var k = Math.Min(1 - red, 1 - green);
            k = Math.Min(k, 1 - blue);
            var c = (1 - red - k) / (1 - k);
            var m = (1 - green - k) / (1 - k);
            var y = (1 - blue - k) / (1 - k);

            return c.ToString("0.00", _numberFormat) + " " +
                   m.ToString("0.00", _numberFormat) + " " +
                   y.ToString("0.00", _numberFormat) + " " +
                   k.ToString("0.00", _numberFormat);
        }

        private void CreateStampFile(string filename, ConversionProfile profile, TokenReplacer tokenReplacer)
        {
            // Create a resource manager to retrieve resources.
            var rm = new ResourceManager(typeof(Resources));

            var stampString = rm.GetString("PostScriptStamp");

            if (stampString == null)
                throw new InvalidOperationException("Error while fetching stamp template");

            var outlineWidth = 0;
            var outlineString = "show";

            if (profile.Stamping.FontAsOutline)
            {
                outlineWidth = profile.Stamping.FontOutlineWidth;
                outlineString = "true charpath stroke";
            }

            var textWithTokens = tokenReplacer.ReplaceTokens(profile.Stamping.StampText);
            var stampText = RemoveIllegalCharacters(textWithTokens);

            // Only Latin1 chars are allowed here
            stampString = stampString.Replace("[STAMPSTRING]",
                EncodeGhostscriptParametersOctal(stampText));
            stampString = stampString.Replace("[FONTNAME]", profile.Stamping.PostScriptFontName);
            stampString = stampString.Replace("[FONTSIZE]", profile.Stamping.FontSize.ToString(_numberFormat));
            stampString = stampString.Replace("[STAMPOUTLINEFONTTHICKNESS]", outlineWidth.ToString(CultureInfo.InvariantCulture));
            stampString = stampString.Replace("[USEOUTLINEFONT]", outlineString); // true charpath stroke OR show

            if (profile.OutputFormat == OutputFormat.PdfX ||
                profile.PdfSettings.ColorModel == ColorModel.Cmyk)
            {
                var colorString = RgbToCmykColorString(profile.Stamping.Color);
                stampString = stampString.Replace("[FONTCOLOR]", colorString);
                stampString = stampString.Replace("setrgbcolor", "setcmykcolor");
            }
            else
            {
                var colorString = (profile.Stamping.Color.R / 255.0).ToString("0.00", _numberFormat) + " " +
                                  (profile.Stamping.Color.G / 255.0).ToString("0.00", _numberFormat) + " " +
                                  (profile.Stamping.Color.B / 255.0).ToString("0.00", _numberFormat);
                stampString = stampString.Replace("[FONTCOLOR]", colorString);
            }

            FileWrap.WriteAllText(filename, stampString);
        }

        private string RemoveIllegalCharacters(string text)
        {
            var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            return Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
        }

        protected string EncodeGhostscriptParametersOctal(string String)
        {
            var sb = new StringBuilder();

            foreach (var c in String)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '{':
                        sb.Append("\\{");
                        break;

                    case '}':
                        sb.Append("\\}");
                        break;

                    case '[':
                        sb.Append("\\[");
                        break;

                    case ']':
                        sb.Append("\\]");
                        break;

                    case '(':
                        sb.Append("\\(");
                        break;

                    case ')':
                        sb.Append("\\)");
                        break;

                    default:
                        int charCode = c;
                        if (charCode > 127)
                            sb.Append("\\" + Convert.ToString(Math.Min(charCode, 255), 8));
                        else sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        protected string EncodeGhostscriptParametersHex(string String)
        {
            if (String == null)
                return "()";

            return "<FEFF" + BitConverter.ToString(Encoding.BigEndianUnicode.GetBytes(String)).Replace("-", "") + ">";
        }

        /// <summary>
        ///     This functions is called by inherited classes to add device-specific parameters to the Ghostscript parameter list
        /// </summary>
        /// <param name="parameters">The current list of parameters. This list may be modified in inherited classes.</param>
        protected abstract void AddDeviceSpecificParameters(IList<string> parameters);

        protected abstract string ComposeOutputFilename();

        private void AddViewerSettingsToMetadataContent(StringBuilder metadataContent)
        {
            metadataContent.Append("\n[\n/PageLayout ");

            switch (Job.Profile.PdfSettings.PageView)
            {
                case PageView.OneColumn:
                    metadataContent.Append("/OneColumn");
                    break;

                case PageView.TwoColumnsOddLeft:
                    metadataContent.Append("/TwoColumnLeft");
                    break;

                case PageView.TwoColumnsOddRight:
                    metadataContent.Append("/TwoColumnRight");
                    break;

                case PageView.TwoPagesOddLeft:
                    metadataContent.Append("/TwoPageLeft");
                    break;

                case PageView.TwoPagesOddRight:
                    metadataContent.Append("/TwoPageRight");
                    break;

                case PageView.OnePage:
                    metadataContent.Append("/SinglePage");
                    break;
            }

            metadataContent.Append("\n/PageMode ");
            switch (Job.Profile.PdfSettings.DocumentView)
            {
                case DocumentView.AttachmentsPanel:
                    metadataContent.Append("/UseAttachments");
                    break;

                case DocumentView.ContentGroupPanel:
                    metadataContent.Append("/UseOC");
                    break;

                case DocumentView.FullScreen:
                    metadataContent.Append("/FullScreen");
                    break;

                case DocumentView.Outline:
                    metadataContent.Append("/UseOutlines");
                    break;

                case DocumentView.ThumbnailImages:
                    metadataContent.Append("/UseThumbs");
                    break;

                default:
                    metadataContent.Append("/UseNone");
                    break;
            }

            if (Job.Profile.PdfSettings.ViewerStartsOnPage > Job.NumberOfPages)
                metadataContent.Append(" /Page " + Job.NumberOfPages);
            else if (Job.Profile.PdfSettings.ViewerStartsOnPage <= 0)
                metadataContent.Append(" /Page 1");
            else
                metadataContent.Append(" /Page " + Job.Profile.PdfSettings.ViewerStartsOnPage);

            metadataContent.Append("\n/DOCVIEW pdfmark");
        }
    }
}
