using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewSecondAnnotCreator : ViewAnnotationCreator {
    public TransViewSecondAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
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
            var pylon = SheetInfo.HostElems.Last();

            // Вертикальные размеры
            var vertDimensionService = new TransViewVertDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon,
                                                                         dimensionBaseService);
            bool longGridsWillBeNeeded = vertDimensionService.TryCreateDimensions(false, pylon, grids);

            // Горизонтальные размеры
            var horizDimensionService = new TransViewHorizDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon,
                                                                           dimensionBaseService);
            horizDimensionService.TryCreateDimensions(false, pylon, grids);

            // Настраиваем положения концов осей на виде
            EditGridEnds(pylon, grids, longGridsWillBeNeeded, dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateTransverseViewBarMarks();
        } catch(Exception) { }
    }


    /// <summary>
    /// Редактирует положения концов осей на виде
    /// </summary>
    private void EditGridEnds(Element pylon, List<Grid> grids, bool longGridsWillBeNeeded,
                              DimensionBaseService dimensionBaseService) {
        var transverseViewGridOffsets = new OffsetOption {
            LeftOffset = 1.5,
            RightOffset = 0.3,
            TopOffset = 0.2,
            BottomOffset = 1.6
        };

        if(longGridsWillBeNeeded) {
            transverseViewGridOffsets.BottomOffset = 3.0;
        }
        var gridEndsService = new GridEndsService(ViewOfPylon.ViewElement, dimensionBaseService);
        gridEndsService.EditGridEnds(pylon, grids, transverseViewGridOffsets);
    }
}
