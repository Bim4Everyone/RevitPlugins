using System;
using System.Linq;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewFirstAnnotCreator : ViewAnnotationCreator {
    public TransViewFirstAnnotCreator(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                        PylonView pylonView) 
        : base(settings, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var doc = view.Document;
        var dimensionBaseService = new DimensionBaseService(view, SheetInfo.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            var grids = Repository.GridsInView(view);
            // Определяем относительно какого пилона нужны размеры - верхнего или нижнего 
            var pylon = SheetInfo.HostElems.First();

            // Вертикальные размеры
            var vertDimensionService = new TransViewVertDimensionService(Settings, doc, SheetInfo, ViewOfPylon, 
                                                                         dimensionBaseService);

            // Если используется армирование для паркинга, то нужно ставить от армирования, потому что есть выпуска вниз
            bool dimensionLineFromPylon = SheetInfo.RebarInfo.SkeletonParentRebarForParking ? false : true;

            bool longGridsWillBeNeeded = vertDimensionService.TryCreateDimensions(false, dimensionLineFromPylon,
                                                                                  pylon, grids);
            // Горизонтальные размеры
            var horizDimensionService = new TransViewHorizDimensionService(Settings, doc, SheetInfo, ViewOfPylon,
                                                                           dimensionBaseService);
            horizDimensionService.TryCreateDimensions(false, pylon, grids);

            // Настраиваем положения концов осей на виде
            var gridEndsService = new GridEndsService(view, dimensionBaseService);
            gridEndsService.EditTransViewGridEnds(pylon, grids, longGridsWillBeNeeded);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewMarkService(Settings, doc, SheetInfo, ViewOfPylon);
            markService.TryCreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
