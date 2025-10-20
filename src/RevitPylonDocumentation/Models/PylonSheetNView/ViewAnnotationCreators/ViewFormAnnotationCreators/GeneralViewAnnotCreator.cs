using System;
using System.Linq;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewAnnotCreator : ViewAnnotationCreator {
    public GeneralViewAnnotCreator(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                   PylonView pylonView) 
        : base(settings, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var doc = view.Document;
        var dimensionBaseService = new DimensionBaseService(view, SheetInfo.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            // Получаем родительское семейство вертикальных стержней на виде
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Получаем оси на виде
            var grids = Repository.GridsInView(view);

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            var vertDimensionService = new GeneralViewVertDimensionService(Settings, doc, SheetInfo,
                                                                           ViewOfPylon, dimensionBaseService);
            vertDimensionService.CreateDimensions(skeletonParentRebar, grids, false);
            
            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            var horizDimensionService = new GeneralViewHorizDimensionService(Settings, doc, SheetInfo, 
                                                                             ViewOfPylon, dimensionBaseService);
            horizDimensionService.CreateDimensions(skeletonParentRebar, false);

            // Настраиваем положения концов осей на виде
            var gridEndsService = new GridEndsService(view, dimensionBaseService);
            gridEndsService.EditGeneralViewGridEnds(SheetInfo.HostElems.First(), grids);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewMarkService(Settings, doc, SheetInfo, ViewOfPylon, 
                                                         dimensionBaseService);
            markService.TryCreatePylonElevMark(SheetInfo.HostElems);
            markService.TryCreateSkeletonMark(false);
            markService.TryCreateAdditionalMark(false);
            markService.TryCreateConcretingSeams();

            var breakLineService = new GeneralViewBreakLineMarkService(Settings, doc, SheetInfo, ViewOfPylon);
            breakLineService.TryCreateLowerBreakLines(false);
            breakLineService.TryCreateUpperBreakLines();
            breakLineService.TryCreateMiddleBreakLines(false);
        } catch(Exception) { }
    }
}
