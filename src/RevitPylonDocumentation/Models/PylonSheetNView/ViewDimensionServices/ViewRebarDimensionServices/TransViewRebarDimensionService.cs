using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarDimensionService {
    internal TransViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }

    internal void TryCreateTransViewRebarDimensions(View view, bool onTopOfRebar) {
        var offsetType = onTopOfRebar ? DimensionOffsetType.Top : DimensionOffsetType.Bottom;

        var doc = Repository.Document;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, offsetType, 0.5);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт" });
            var dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            var dimensionLineBottomEdge = dimensionBaseService.GetDimensionLine(skeletonParentRebar, offsetType, 1);
            var refArrayBottomEdge = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            var dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge, ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            var dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            var refArrayRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                new List<string>() { "низ", "торец", "край" });
            var dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }
}
