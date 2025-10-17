using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSetCoordParams.Models;

internal class RevitRepository {

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    //Метод получения типа "Связанный файл"
    //public IList<RevitElement> GetLinkTypeElements() {
    //    var listRevitLinkTypes = new FilteredElementCollector(Document)
    //        .OfCategory(BuiltInCategory.OST_RvtLinks)
    //        .OfClass(typeof(RevitLinkType))
    //        .Cast<RevitLinkType>()
    //        .Where(linkType => !linkType.IsNestedLink)
    //        .ToList();
    //    return new Collection<LinkTypeElement>(listRevitLinkTypes
    //        .Select(linkType => new LinkTypeElement(linkType))
    //        .ToList());
    //}
}
