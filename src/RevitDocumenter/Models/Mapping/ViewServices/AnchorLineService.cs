using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Mapping.ViewServices;
internal class AnchorLineService {
    private readonly RevitRepository _revitRepository;

    public AnchorLineService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public List<ElementId> CreateAnchorLines(XYZ viewMinFixed, XYZ viewMaxFixed, int lineWeight, Color lineColor) {
        var overrideSettings = new OverrideGraphicSettings();
        overrideSettings.SetProjectionLineWeight(lineWeight);
        overrideSettings.SetProjectionLineColor(lineColor);

        return [
            CreateLineWithOverrides(viewMinFixed, viewMinFixed + XYZ.BasisX, overrideSettings).Id,
            CreateLineWithOverrides(viewMaxFixed, viewMaxFixed - XYZ.BasisX, overrideSettings).Id
        ];
    }

    public DetailCurve CreateLineWithOverrides(XYZ pt1, XYZ pt2, OverrideGraphicSettings overrideSettings) {
        var doc = _revitRepository.Document;

        var lineGeom = Line.CreateBound(pt1, pt2);
        using var subTransaction = new SubTransaction(doc);
        subTransaction.Start();

        var detailLine = doc.Create.NewDetailCurve(doc.ActiveView, lineGeom);
        _revitRepository.Document.ActiveView.SetElementOverrides(detailLine.Id, overrideSettings);

        subTransaction.Commit();
        return detailLine;
    }
}
