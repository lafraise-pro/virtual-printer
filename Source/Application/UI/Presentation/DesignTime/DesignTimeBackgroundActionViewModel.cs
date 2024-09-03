﻿using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.ModifyActions.Background;
using pdfforge.PDFCreator.Utilities.Pdf;

namespace pdfforge.PDFCreator.UI.Presentation.DesignTime
{
    public class DesignTimeBackgroundActionViewModel : BackgroundActionViewModel
    {
        public DesignTimeBackgroundActionViewModel()
            : base(new DesignTimeActionLocator(),
                null,
                new DesignTimeTranslationUpdater(),
                new DesignTimeCurrentSettingsProvider(),
                null,
                null,
                null,
                null,
                new PdfVersionHelper(),
                null,
                null)
        {
        }
    }
}
