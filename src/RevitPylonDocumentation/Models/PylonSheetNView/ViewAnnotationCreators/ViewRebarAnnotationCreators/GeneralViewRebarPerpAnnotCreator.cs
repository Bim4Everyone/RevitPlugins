using System;
using System.Collections.Generic;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarPerpAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarPerpAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }


    public override void TryCreateViewAnnotations() {

        var doc = Repository.Document;
        var view = SheetInfo.GeneralViewPerpendicularRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        var dimensionService = new GeneralViewPerpRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "торец" });
            var dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top);
            var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "торец" });
            var dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);



            var defaultDimensionOffsetType = DimensionOffsetType.Right;
            // Будем ставить по дефолту справа
            // Слева будем ставить только если есть гэшка (но не все) и она справа

            if(SheetInfo.RebarInfo.HasLRebar && dimensionService.LRebarIsRight(view, rebarFinder)) {
                defaultDimensionOffsetType = DimensionOffsetType.Left;
            }

            var plates = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);
            dimensionService.CreateGeneralRebarViewPlateDimensions(view, skeletonParentRebar, plates, defaultDimensionOffsetType, dimensionBaseService);



            if(!SheetInfo.RebarInfo.AllRebarAreL && SheetInfo.RebarInfo.HasLRebar) {
                // #1_горизонт_Г-стержень
                var refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "Г-стержень"]);
                // #_1_горизонт_край_низ
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "край", "низ"], refArraySide);

                defaultDimensionOffsetType = defaultDimensionOffsetType == DimensionOffsetType.Left ? DimensionOffsetType.Right : DimensionOffsetType.Left;

                var dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, defaultDimensionOffsetType, 1.3);
                var dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, refArraySide, ViewModel.SelectedDimensionType);
            }

            dimensionService.TryCreateGeneralRebarPerpendicularViewAdditionalDimensions();
        } catch(Exception) { }
    }
}
