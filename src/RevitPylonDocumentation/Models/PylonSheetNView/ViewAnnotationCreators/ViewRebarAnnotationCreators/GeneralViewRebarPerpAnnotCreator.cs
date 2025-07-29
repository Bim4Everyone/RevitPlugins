using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarPerpAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarPerpAnnotCreator(MainViewModel mvm, RevitRepository repository, 
                                              PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }


    public override void TryCreateViewAnnotations() {
        var dimensionService = new GeneralViewRebarPerpDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);
        var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, ViewModel.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Создаем размеры по вертикальным стержням снизу и сверху (если нужно)
            dimensionService.TryCreateTopEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.TryCreateBottomEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);

            // Определяем с какой стороны строить вертикальную размерную цепочку по пластинам
            var plateDimensionOffsetType = GetPlateDimensionOffsetType(dimensionService);
            // Создаем размерную цепочку по пластинам
            dimensionService.TryCreatePlateDimensions(skeletonParentRebar, plateDimensionOffsetType, 
                                                      dimensionBaseService);
            // Если Г-образный стержень только с одной стороны, то его нужно образмерить
            // В противном случае (оба Г-образные) это произойдет ранее
            if(!SheetInfo.RebarInfo.AllRebarAreL && SheetInfo.RebarInfo.HasLRebar) {
                var rebarDimensionOffsetType = plateDimensionOffsetType == 
                    DimensionOffsetType.Left ? DimensionOffsetType.Right : DimensionOffsetType.Left;
                dimensionService.TryCreateLRebarDimension(skeletonParentRebar, rebarDimensionOffsetType, 
                                                          dimensionBaseService);
            }
            dimensionService.TryCreateLRebarDimension(skeletonParentRebar, dimensionBaseService);
            // Создаем размеры по изгибам вертикальных стержней-бутылок
            dimensionService.TryCreateAdditionalDimensions(skeletonParentRebar, dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {

        } catch(Exception) { }
    }

    private DimensionOffsetType GetPlateDimensionOffsetType(GeneralViewRebarPerpDimensionService dimensionService) {
        // Будем ставить размерную цепочку по дефолту справа
        var plateDimensionOffsetType = DimensionOffsetType.Right;
        // Слева будем ставить только если есть Г-образный стержень (но не все) и он справа
        if(SheetInfo.RebarInfo.HasLRebar && dimensionService.LRebarIsRight()) {
            plateDimensionOffsetType = DimensionOffsetType.Left;
        }
        return plateDimensionOffsetType;
    }
}
