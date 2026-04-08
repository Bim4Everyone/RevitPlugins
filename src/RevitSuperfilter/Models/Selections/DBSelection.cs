using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSuperfilter.Models.Selections;

internal sealed class DBSelection : BaseSelection {
    public DBSelection(UIApplication uiApplication)
        : base(uiApplication) {
    }

    public override Selection Selection => Selection.DBSelection;

    public override IEnumerable<Element> GetElements() {
        return new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .ToElements();
    }
}
