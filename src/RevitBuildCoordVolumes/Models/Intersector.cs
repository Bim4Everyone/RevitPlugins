using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;
internal class Intersector {

    private readonly View3D _view3D;
    private readonly Document _document;
    private readonly XYZ _direction = new(0, 0, 10);

    private readonly ICollection<string> _krNames = [
        "(КР)",
        "(К)",
        "(K)",
        "КЖ",
        "кж",
        "ЖБ",
        "жб",
        "Плита",
        "плита",
        "Железобетон",
        "железобетон",
        "Монолит",
        "монолит",
        "ерекрытие"];

    private readonly ICollection<BuiltInCategory> _krCategories = [
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Floors];

    public Intersector(View3D view3D, Document document) {
        _view3D = view3D;
        _document = document;
    }

    public double GetIntersectLocation(XYZ origin, ReferenceIntersector referenceIntersector) {
        var referenceWithContext = referenceIntersector.FindNearest(origin, _direction);
        if(referenceWithContext != null) {
            var reference = referenceWithContext.GetReference();
            if(reference != null) {
                var linkInstance = _document.GetElement(reference) as RevitLinkInstance;
                var transform = linkInstance.GetTransform();
                var linkDoc = linkInstance.GetLinkDocument();
                var linkElement = linkDoc.GetElement(reference.LinkedElementId);
                var linkLevel = linkDoc.GetElement(linkElement.LevelId) as Level;

                double levelElevation = linkLevel.Elevation;
                double offset = linkElement.LookupParameter("Смещение от уровня").AsDouble();
                return levelElevation + offset;
            }
        }
        return origin.Z;
    }


    public ReferenceIntersector GetIntersector() {
        var multicategoryFilter = new ElementMulticategoryFilter(_krCategories);
        var pvp = new ParameterValueProvider(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME));
        var fsc = new FilterStringContains();

        var epf = new List<ElementFilter>();
        foreach(string krName in _krNames) {
            var rule = new FilterStringRule(pvp, fsc, krName);
            var pf = new ElementParameterFilter(rule);
            epf.Add(pf);
        }

        var logicalOrFilter = new LogicalOrFilter(epf);
        var logicalAndFilter = new LogicalAndFilter(logicalOrFilter, multicategoryFilter);

        var intersector = new ReferenceIntersector(logicalAndFilter, FindReferenceTarget.Face, _view3D) {
            FindReferencesInRevitLinks = true
        };
        return intersector;
    }
}
