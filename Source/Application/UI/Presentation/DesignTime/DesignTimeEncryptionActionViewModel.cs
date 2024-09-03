﻿using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.ModifyActions.Encryption;

namespace pdfforge.PDFCreator.UI.Presentation.DesignTime
{
    public class DesignTimeEncryptionActionViewModel : EncryptionActionViewModel
    {
        public DesignTimeEncryptionActionViewModel()
            : base(new DesignTimeTranslationUpdater(), new DesignTimeCurrentSettingsProvider(), null,
                new DesignTimeEditionHelper(), new DesignTimeActionLocator(), null, null, null, null)
        {
        }
    }
}
