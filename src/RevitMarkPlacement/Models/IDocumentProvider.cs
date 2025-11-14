using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitMarkPlacement.Models;

internal interface IDocumentProvider {
    Document GetDocument();
    UIDocument GetUIDocument();
}
