using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSuperfilter.Models.Selections;

internal sealed class DBViewSelection : BaseSelection, ISelectionElements {
    public DBViewSelection(UIApplication uiApplication)
        : base(uiApplication) {
    }

    public override Selection Selection => Selection.DBViewSelection;

    public override IEnumerable<Element> GetElements() {
        var document = Document;
        return new FilteredElementCollector(document, document.ActiveView.Id)
            .WhereElementIsNotElementType()
            .ToElements();
    }
}
