using System;
using System.Linq;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewAnnotCreator : ViewAnnotationCreator {
    public GeneralViewAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                   PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

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
            var vertDimensionService = new GeneralViewVertDimensionService(ViewModel, Repository, SheetInfo,
                                                                           ViewOfPylon, dimensionBaseService);
            vertDimensionService.CreateDimensions(skeletonParentRebar, grids, false);
            
            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            var horizDimensionService = new GeneralViewHorizDimensionService(ViewModel, Repository, SheetInfo, 
                                                                             ViewOfPylon, dimensionBaseService);
            horizDimensionService.CreateDimensions(skeletonParentRebar, false);

            // Настраиваем положения концов осей на виде
            var gridEndsService = new GridEndsService(view, dimensionBaseService);
            gridEndsService.EditGeneralViewGridEnds(SheetInfo.HostElems.First(), grids);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon, 
                                                         dimensionBaseService);
            markService.TryCreatePylonElevMark(SheetInfo.HostElems);
            markService.TryCreateSkeletonMark(false);
            markService.TryCreateAdditionalMark(false);
            markService.TryCreateConcretingSeams();

            var breakLineService = new GeneralViewBreakLineMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon, 
                                                                       dimensionBaseService);
            breakLineService.TryCreateLowerBreakLines(false);
            breakLineService.TryCreateUpperBreakLines();
            breakLineService.TryCreateMiddleBreakLines(false);
        } catch(Exception) { }
    }
}
