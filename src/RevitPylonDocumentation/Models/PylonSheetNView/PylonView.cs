using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonView {

    internal PylonView(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                       IAnnotationCreatorFactory annotationCreatorFactory = null) {
        Settings = settings;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        var doc = repository.Document;

        ViewSectionCreator = new PylonViewSectionCreator(settings, doc, pylonSheetInfo);
        ViewScheduleCreator = new PylonViewScheduleCreator(settings, doc, pylonSheetInfo);
        
        AnnotationCreator = annotationCreatorFactory?.CreateAnnotationCreator(settings, repository, pylonSheetInfo, this);

        ViewSectionPlacer = new PylonViewSectionPlacer(settings, doc, pylonSheetInfo);
        ViewSchedulePlacer = new PylonViewSchedulePlacer(settings, doc, pylonSheetInfo);
        LegendPlacer = new PylonViewLegendPlacer(settings, doc, pylonSheetInfo);
    }

    internal CreationSettings Settings { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }

    public View ViewElement { get; set; }
    public string ViewName { get; set; }
    public int ViewScale { get; set; }
    public Element ViewportElement { get; set; }
    public string ViewportNumber { get; set; }
    public string ViewportName { get; set; }
    public string ViewportTypeName { get; set; }
    public double ViewportHalfWidth { get; set; }
    public double ViewportHalfHeight { get; set; }
    public XYZ ViewportCenter { get; set; }


    public PylonViewSectionCreator ViewSectionCreator { get; set; }
    public PylonViewScheduleCreator ViewScheduleCreator { get; set; }

    public ViewAnnotationCreator AnnotationCreator { get; set; }

    public PylonViewSectionPlacer ViewSectionPlacer { get; set; }
    public PylonViewSchedulePlacer ViewSchedulePlacer { get; set; }
    public PylonViewLegendPlacer LegendPlacer { get; set; }
}
