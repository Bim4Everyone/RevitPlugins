using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewFirstRebarAnnotCreator : ViewAnnotationCreator {
    internal TransViewFirstRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, 
                                             PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            // Если используется армирование для паркинга, то нужно ставить от армирования, потому что есть выпуска вниз
            bool dimensionLineFromPylon = SheetInfo.RebarInfo.SkeletonParentRebarForParking ? false : true;

            var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, ViewModel.ParamValService);
            var dimensionService = new TransViewRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon,
                                                                      dimensionBaseService);
            dimensionService.TryCreateTransViewRebarDimensions(false, dimensionLineFromPylon);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateBarMarks();
        } catch(Exception) { }
    }
}
