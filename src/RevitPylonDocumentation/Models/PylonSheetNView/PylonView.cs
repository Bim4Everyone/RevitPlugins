using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonView {

    internal PylonView(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                       IAnnotationCreatorFactory annotationCreatorFactory = null) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;

        ViewSectionCreator = new PylonViewSectionCreator(mvm, repository, pylonSheetInfo);
        ViewScheduleCreator = new PylonViewScheduleCreator(mvm, repository, pylonSheetInfo);
        
        DimensionCreator = annotationCreatorFactory?.CreateDimensionCreator(mvm, repository, pylonSheetInfo, this);
        MarkCreator = annotationCreatorFactory?.CreateMarkCreator(mvm, repository, pylonSheetInfo, this);

        ViewSectionPlacer = new PylonViewSectionPlacer(mvm, repository, pylonSheetInfo);
        ViewSchedulePlacer = new PylonViewSchedulePlacer(mvm, repository, pylonSheetInfo);
        LegendPlacer = new PylonViewLegendPlacer(mvm, repository, pylonSheetInfo);
    }

    internal MainViewModel ViewModel { get; set; }
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

    public ViewDimensionCreator DimensionCreator { get; set; }
    public ViewMarkCreator MarkCreator { get; set; }

    public PylonViewSectionPlacer ViewSectionPlacer { get; set; }
    public PylonViewSchedulePlacer ViewSchedulePlacer { get; set; }
    public PylonViewLegendPlacer LegendPlacer { get; set; }
}
