using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models;
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public View3D GetView3D(string familyName) {
        string name3Dview = string.Format(
            _localizationService.GetLocalizedString("RevitRepository.ViewName"), Application.Username, familyName);

        var view = new FilteredElementCollector(Document)
            .OfClass(typeof(View3D))
            .Where(v => v.Name.Equals(name3Dview, StringComparison.OrdinalIgnoreCase))
            .Cast<View3D>()
            .FirstOrDefault();

        return view ?? CreateView3D(name3Dview);
    }

    public List<Room> GetSelectedRooms() {
        return ActiveUIDocument.GetSelectedElements()
            .OfType<Room>()
            .ToList();
    }

    public void SetSelectedRoom(ElementId elementId) {
        List<ElementId> listRoomElements = [
            elementId ];
        ActiveUIDocument.SetSelectedElements(listRoomElements);
    }

    public FamilySymbol GetFamilySymbol(Family family) {
        ElementFilter filter = new FamilySymbolFilter(family.Id);
        return new FilteredElementCollector(Document)
            .WherePasses(filter)
            .Cast<FamilySymbol>()
            .First();
    }

    private View3D CreateView3D(string name3Dview) {
        var viewTypes = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>();
        var viewTypes3D = viewTypes
            .Where(vt => vt.ViewFamily == ViewFamily.ThreeDimensional)
            .First();
        string transactionName = _localizationService.GetLocalizedString("RevitRepository.TransactionNameCreate");
        using var t = Document.StartTransaction(transactionName);
        var view3D = View3D.CreateIsometric(Document, viewTypes3D.Id);
        view3D.Name = name3Dview;
        t.Commit();
        return view3D;
    }

    public double GetBasePointLocation() {
        var basePoint = BasePoint.GetProjectBasePoint(Document);
        return basePoint.Position.Z;
    }
}
