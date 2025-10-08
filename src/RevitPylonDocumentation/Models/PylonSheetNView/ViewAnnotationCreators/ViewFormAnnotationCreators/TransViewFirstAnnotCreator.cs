using System;
using System.Linq;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewFirstAnnotCreator : ViewAnnotationCreator {
    internal TransViewFirstAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                        PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            var view = ViewOfPylon.ViewElement;
            var grids = Repository.GridsInView(view);
            var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

            // Определяем относительно какого пилона нужны размеры - верхнего или нижнего 
            var pylon = SheetInfo.HostElems.First();

            // Вертикальные размеры
            var vertDimensionService = new TransViewVertDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon, 
                                                                         dimensionBaseService);

            // Если используется армирование для паркинга, то нужно ставить от армирования, потому что есть выпуска вниз
            bool dimensionLineFromPylon = SheetInfo.RebarInfo.SkeletonParentRebarForParking ? false : true;

            bool longGridsWillBeNeeded = vertDimensionService.TryCreateDimensions(false, dimensionLineFromPylon,
                                                                                  pylon, grids);
            // Горизонтальные размеры
            var horizDimensionService = new TransViewHorizDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon,
                                                                           dimensionBaseService);
            horizDimensionService.TryCreateDimensions(false, pylon, grids);

            // Настраиваем положения концов осей на виде
            var gridEndsService = new GridEndsService(view, dimensionBaseService);
            gridEndsService.EditTransViewGridEnds(pylon, grids, longGridsWillBeNeeded);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
