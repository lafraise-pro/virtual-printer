﻿using NLog;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace pdfforge.PDFCreator.Conversion.Actions.Actions
{
    public interface IHttpAction
    {
        ActionResult CheckAccount(HttpAccount httpAccount, bool autoSave, CheckLevel checkLevel);
    }

    public class HttpAction : RetypePasswordActionBase<HttpSettings>, IPostConversionAction, IHttpAction
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HttpAction() : base(p => p.HttpSettings)
        { }

        protected override string PasswordText => "HTTP";

        public override void ApplyPreSpecifiedTokens(Job job)
        {
            //the token in the url is replaced directly in the action
            //therefore we don't have to deal with account copies
        }

        /// <summary>
        ///     Check if the profile is configured properly for this action
        /// </summary>
        /// <param name="profile">The profile to check</param>
        /// <param name="settings">Current settings</param>
        /// <param name="checkLevel"></param>
        /// <returns>ActionResult with configuration problems</returns>
        public override ActionResult Check(ConversionProfile profile, CurrentCheckSettings settings, CheckLevel checkLevel)
        {
            var actionResult = new ActionResult();

            if (!IsEnabled(profile))
                return actionResult;

            var httpAccount = settings.Accounts.GetHttpAccount(profile);

            return CheckAccount(httpAccount, profile.AutoSave.Enabled, checkLevel);
        }

        public ActionResult CheckAccount(HttpAccount httpAccount, bool isAutoSave, CheckLevel checkLevel)
        {
            var actionResult = new ActionResult();

            if (httpAccount == null)
            {
                actionResult.Add(ErrorCode.HTTP_NoAccount);
                return actionResult;
            }

            var startsWithToken = httpAccount.Url.StartsWith("<") && httpAccount.Url.Contains(">");
            var containsToken = httpAccount.Url.Contains("<") && httpAccount.Url.Contains(">");

            if (string.IsNullOrWhiteSpace(httpAccount.Url))
            {
                actionResult.Add(ErrorCode.HTTP_MissingOrInvalidUrl);
            }
            else if (startsWithToken)
            {
            }
            else if (containsToken)
            {
                if (!httpAccount.Url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                    && !httpAccount.Url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                {
                    actionResult.Add(ErrorCode.HTTP_MustStartWithHttp);
                }
            }
            else
            {
                if (!Uri.TryCreate(httpAccount.Url, UriKind.Absolute, out var isValidUrl))
                {
                    actionResult.Add(ErrorCode.HTTP_MissingOrInvalidUrl);
                }
                else if (isValidUrl.Scheme != Uri.UriSchemeHttp && isValidUrl.Scheme != Uri.UriSchemeHttps)
                {
                    actionResult.Add(ErrorCode.HTTP_MustStartWithHttp);
                }
            }

            if (httpAccount.IsBasicAuthentication)
            {
                if (string.IsNullOrWhiteSpace(httpAccount.UserName))
                    actionResult.Add(ErrorCode.HTTP_NoUserNameForAuth);

                if (isAutoSave && string.IsNullOrWhiteSpace(httpAccount.Password))
                    actionResult.Add(ErrorCode.HTTP_NoPasswordForAuthWithAutoSave);
            }

            return actionResult;
        }

        protected override ActionResult DoActionProcessing(Job job)
        {
            Logger.Debug("Launched httpRequest-Action");
            try
            {
                return HttpUpload(job);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Exception while upload file to http: ");

                return new ActionResult(ErrorCode.HTTP_Generic_Error);
            }
        }

        private ActionResult HttpUpload(Job job)
        {
            var result = new ActionResult();

            // setup and send request
            var uploadFileViaHttp = UploadFileViaHttp(job);

            // wait for the result
            var httpResponse = uploadFileViaHttp;
            if (httpResponse.IsSuccessStatusCode == false)
            {
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        result.Add(ErrorCode.PasswordAction_Login_Error);
                        break;

                    default:
                        result.Add(ErrorCode.HTTP_Generic_Error);
                        break;
                }
            }
            return result;
        }

        private HttpResponseMessage UploadFileViaHttp(Job job)
        {
            try
            {
                var multiContent = new MultipartContent();
                foreach (var jobOutputFile in job.OutputFiles)
                {
                    var fileStream = new FileStream(jobOutputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var outputFile = new StreamContent(fileStream);
                    var mimeMapping = MimeMapping.GetMimeMapping(jobOutputFile);
                    outputFile.Headers.ContentType = new MediaTypeHeaderValue(mimeMapping);
                    outputFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                    outputFile.Headers.ContentDisposition.FileName = Path.GetFileName(jobOutputFile);
                    outputFile.Headers.ContentDisposition.Name = GetRandomString(12);
                    multiContent.Add(outputFile);
                }

                // do the Post request and wait for an answer
                return MakePostRequest(job, multiContent).Result;
            }
            catch (Exception e)
            {
                Logger.Error($"Exception during HTTP upload:\r\n{e.Message}");
                throw;
            }
        }

        private string GetRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        private async Task<HttpResponseMessage> MakePostRequest(Job job, HttpContent message)
        {
            var account = job.Accounts.GetHttpAccount(job.Profile);
            var httpClient = new HttpClient();
            var timeout = account.Timeout;

            if (timeout < 0)
                timeout = 60;

            httpClient.Timeout = TimeSpan.FromSeconds(timeout);

            var url = job.TokenReplacer.ReplaceTokens(account.Url);
            Logger.Debug("Http upload url: " + url);
            var uri = new Uri(url);
            if (account.IsBasicAuthentication)
            {
                var asciiAuth = Encoding.ASCII.GetBytes($"{account.UserName}:{job.Passwords.HttpPassword}");
                var endcoded = Convert.ToBase64String(asciiAuth);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", endcoded);
            }
            return await httpClient.PostAsync(uri, message).ConfigureAwait(false);
        }

        protected override void SetPassword(Job job, string password)
        {
            job.Passwords.HttpPassword = password;
        }

        public override bool IsRestricted(ConversionProfile profile)
        {
            return false;
        }

        protected override void ApplyActionSpecificRestrictions(Job job)
        { }
    }
}
