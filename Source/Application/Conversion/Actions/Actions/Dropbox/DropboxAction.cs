﻿using NLog;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Utilities;
using pdfforge.PDFCreator.Utilities.Tokens;
using System.Linq;

namespace pdfforge.PDFCreator.Conversion.Actions.Actions.Dropbox
{
    public class DropboxAction : ActionBase<DropboxSettings>, IPostConversionAction
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDropboxService _dropboxService;

        public DropboxAction(IDropboxService dropboxService)
            : base(p => p.DropboxSettings)
        {
            _dropboxService = dropboxService;
        }

        protected override ActionResult DoProcessJob(Job job, IPdfProcessor processor)
        {
            _logger.Debug("Launched Dropbox Action");

            var settings = new CurrentCheckSettings(job.AvailableProfiles, job.PrinterMappings, job.Accounts);
            var actionResult = Check(job.Profile, settings, CheckLevel.RunningJob);
            if (!actionResult)
                return actionResult;

            var sharedLink = job.Profile.DropboxSettings.CreateShareLink;
            var shareFolder = job.TokenReplacer.ReplaceTokens(job.Profile.DropboxSettings.SharedFolder);

            var currentDropBoxAccount = job.Accounts.GetDropboxAccount(job.Profile);
            if (sharedLink)
            {
                try
                {
                    var shareLink = _dropboxService.UploadFileWithSharing(
                        currentDropBoxAccount.AccessToken,
                        currentDropBoxAccount.RefreshToken,
                        shareFolder, job.OutputFiles,
                        job.Profile.DropboxSettings.EnsureUniqueFilenames,
                        job.OutputFileTemplate);

                    if (shareLink == null)
                    {
                        return new ActionResult(ErrorCode.Dropbox_Upload_And_Share_Error);
                    }

                    job.ShareLinks.DropboxShareUrl = shareLink.ShareUrl;

                    shareLink.Filename = shareLink.Filename.Split('/').Last();
                    job.TokenReplacer.AddToken(new StringToken("DROPBOXFULLLINKS", $"{shareLink.Filename} ( {shareLink.ShareUrl} )"));
                    job.TokenReplacer.AddToken(new StringToken("DROPBOXHTMLLINKS", $"<a href = '{shareLink.ShareUrl}'>{shareLink.Filename}</a>"));
                }
                catch
                {
                    return new ActionResult(ErrorCode.Dropbox_Upload_And_Share_Error);
                }
            }
            else
            {
                var result = _dropboxService.UploadFiles(
                    currentDropBoxAccount.AccessToken,
                    currentDropBoxAccount.RefreshToken,
                    shareFolder,
                    job.OutputFiles,
                    job.Profile.DropboxSettings.EnsureUniqueFilenames,
                    job.OutputFileTemplate);
                if (result == false)
                {
                    return new ActionResult(ErrorCode.Dropbox_Upload_Error);
                }
            }

            return new ActionResult();
        }

        public override void ApplyPreSpecifiedTokens(Job job)
        {
            job.Profile.DropboxSettings.SharedFolder = job.TokenReplacer.ReplaceTokens(job.Profile.DropboxSettings.SharedFolder)
                                                                        .Replace("\\", "/");
        }

        public override ActionResult Check(ConversionProfile profile, CurrentCheckSettings settings, CheckLevel checkLevel)
        {
            if (!IsEnabled(profile))
                return new ActionResult();

            var isJobLevelCheck = checkLevel == CheckLevel.RunningJob;

            var account = settings.Accounts.GetDropboxAccount(profile);

            if (account == null)
                return new ActionResult(ErrorCode.Dropbox_AccountNotSpecified);

            var accessToken = account.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
                return new ActionResult(ErrorCode.Dropbox_AccessTokenNotSpecified);

            if (!isJobLevelCheck && !profile.UserTokens.Enabled && TokenIdentifier.ContainsUserToken(profile.DropboxSettings.SharedFolder))
                return new ActionResult(ErrorCode.Dropbox_SharedFolder_RequiresUserTokens);

            if (!isJobLevelCheck && TokenIdentifier.ContainsTokens(profile.DropboxSettings.SharedFolder))
                return new ActionResult();

            if (!ValidName.IsValidDropboxFolder(profile.DropboxSettings.SharedFolder))
                return new ActionResult(ErrorCode.Dropbox_InvalidDirectoryName);

            return new ActionResult();
        }

        public override bool IsRestricted(ConversionProfile profile)
        {
            return false;
        }

        protected override void ApplyActionSpecificRestrictions(Job job)
        { }
    }
}
