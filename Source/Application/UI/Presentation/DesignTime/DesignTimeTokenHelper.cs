﻿using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.UI.Presentation.Helper.Tokens;
using pdfforge.PDFCreator.Utilities.Tokens;
using System.Collections.Generic;

namespace pdfforge.PDFCreator.UI.Presentation.DesignTime
{
    public class DesignTimeTokenHelper : ITokenHelper
    {
        public TokenReplacer TokenReplacerWithPlaceHolders => new TokenReplacer();

        public List<string> GetTokenListWithFormatting()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForMetadata()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForFilename()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForDirectory()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForExternalFiles()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForStamp()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForPageNumbers()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForEmail()
        {
            return new List<string>();
        }

        public List<string> GetTokenListForEmailRecipients()
        {
            return new List<string>();
        }

        public TokenWarningCheckResult TokenWarningCheck(string textWithTokens, ConversionProfile profile)
        {
            return TokenWarningCheckResult.NoWarning;
        }
    }
}
