using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
internal class TransViewFirstRebarDimCreator : ViewDimensionCreator {
    internal TransViewFirstRebarDimCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.TransverseViewFirstRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 0.5);
            ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            Line dimensionLineBottomEdge = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1);
            ReferenceArray refArrayBottomEdge = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge, ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                new List<string>() { "низ", "торец", "край" });
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }
}
