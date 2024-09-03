﻿using NLog;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.Enums;

namespace pdfforge.PDFCreator.Conversion.Actions.Actions
{
    public class EncryptionAction : ActionBase<Security>, IConversionAction
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public EncryptionAction() : base(p => p.PdfSettings.Security)
        { }

        protected override ActionResult DoProcessJob(Job job, IPdfProcessor processor)
        {
            //nothing to do here. The Encryption must be triggered as last processing step in the ActionManager
            return new ActionResult();
        }

        public override void ApplyPreSpecifiedTokens(Job job)
        {
            //nothing to do here.
        }

        public override bool IsRestricted(ConversionProfile profile)
        {
            return profile.OutputFormat != OutputFormat.Pdf;
        }

        protected override void ApplyActionSpecificRestrictions(Job job)
        {
            if (!job.Profile.PdfSettings.Security.AllowPrinting)
                job.Profile.PdfSettings.Security.RestrictPrintingToLowQuality = false;
        }

        public override ActionResult Check(ConversionProfile profile, CurrentCheckSettings settings, CheckLevel checkLevel)
        {
            var result = new ActionResult();

            if (!IsEnabled(profile))
                return result;

            var security = profile.PdfSettings.Security;

            if (profile.AutoSave.Enabled)
            {
                if (string.IsNullOrEmpty(security.OwnerPassword))
                {
                    _logger.Error("No saved owner password for security in automatic saving.");
                    result.Add(ErrorCode.AutoSave_NoOwnerPassword);
                }

                if (security.RequireUserPassword)
                {
                    if (string.IsNullOrEmpty(security.UserPassword))
                    {
                        _logger.Error("No saved user password for security in automatic saving.");
                        result.Add(ErrorCode.AutoSave_NoUserPassword);
                    }
                }
            }

            if (checkLevel == CheckLevel.EditingProfile)
                return result;

            return result;
        }
    }
}
