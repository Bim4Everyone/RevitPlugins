using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
internal class TransViewSecondRebarDimCreator : ViewDimensionCreator {
    public TransViewSecondRebarDimCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.TransverseViewSecondRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, 0.5);
            ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "фронт" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);

            Line dimensionLineTopEdge = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, 1);
            ReferenceArray refArrayTopEdge = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                              new List<string>() { "верх", "фронт", "край" });
            Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge, ViewModel.SelectedDimensionType);

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
