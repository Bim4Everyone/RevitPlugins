using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitBuildCoordVolumes.Models;

internal class RevitRepository {

    private readonly View _view;
    private readonly double _minimalSide;
    private readonly double _side;

    public RevitRepository(UIApplication uiApp) {
        UIApplication = uiApp;
        _view = Document.ActiveView;
        _minimalSide = Application.ShortCurveTolerance;
        _side = UnitUtils.ConvertToInternalUnits(300, UnitTypeId.Millimeters);
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public IEnumerable<Element> GetSelectedElements() {
        return ActiveUIDocument.GetSelectedElements();
    }



    public void Process() {
        if(GetSelectedElements().FirstOrDefault() is not Area area) {
            TaskDialog.Show("Ошибка", "Выберите одну зону (Area).");
            return;
        }

        var divider = new AreaDivider();

        var polygons = divider.DivideArea(area, _side, _minimalSide, 5000);

        Draw(polygons);
    }

    private void Draw(List<Polygon> polygons) {
        using var tr = new Transaction(Document, "Draw");
        tr.Start();
        foreach(var polygon in polygons) {
            foreach(var line in polygon.Sides) {
                Document.Create.NewDetailCurve(_view, line);
            }
        }
        tr.Commit();
    }


}

