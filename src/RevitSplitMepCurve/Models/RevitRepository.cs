using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSplitMepCurve.Models;

internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;

    public Document Document => ActiveUIDocument.Document;
}
