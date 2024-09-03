﻿using Translatable;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.WorkflowEditor
{
    public class WorkflowEditorTranslation : ITranslatable
    {
        public string Modify { get; set; } = "Modify";
        public string Send { get; set; } = "Send/Open";

        public string Actions { get; private set; } = "Actions";
        public string AddAction { get; private set; } = "Add action";
        public string Preparation { get; set; } = "Preparation";

        public string AddActionHint { get; private set; } = "Add actions to modify, send or prepare your documents";
    }
}
