using System;
using System.Collections.Generic;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var doc = Repository.Document;
        var view = SheetInfo.GeneralViewRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        var dimensionService = new GeneralViewRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт" });
            var dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            var dimensionLineBottomEdges = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1.5);
            var refArrayBottomEdges = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            var dimensionBottomEdges = doc.Create.NewDimension(view, dimensionLineBottomEdges, refArrayBottomEdges, ViewModel.SelectedDimensionType);

            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top);
            var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "фронт" });
            var dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);


            var plates = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);
            dimensionService.CreateGeneralRebarViewPlateDimensions(view, skeletonParentRebar, plates, DimensionOffsetType.Left, dimensionBaseService);
        } catch(Exception) { }



    }
}
