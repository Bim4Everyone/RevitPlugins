using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

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

            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = SheetInfo.HostElems.First();
            var dimensionLineHostRef = onTopOfRebar ? skeletonParentRebar : pylon;

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            if(!onTopOfRebar) {
                var dimensionLineBottom = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, offsetType, 0.5);
                var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["низ", "фронт"]);
                var dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom,
                                                              ViewModel.SelectedDimensionType);
                dimensionBottom.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
            } else {
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    var dimensionLine = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, offsetType, 0.5);
                    var refArray = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["низ", "фронт"]);
                    var dimension = doc.Create.NewDimension(view, dimensionLine, refArray,
                                                            ViewModel.SelectedDimensionType);
                    dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
                } else if(!SheetInfo.RebarInfo.HasLRebar) {
                    var dimensionLine = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, offsetType, 0.5);
                    var refArray = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["верх", "фронт"]);
                    var dimension = doc.Create.NewDimension(view, dimensionLine, refArray,
                                                                  ViewModel.SelectedDimensionType);
                    dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
                } else {
                    var dimensionLineBottom = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, offsetType, 0.5);
                    var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["низ", "фронт"]);
                    var dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom,
                                                                  ViewModel.SelectedDimensionType);
                    dimensionBottom.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);

                    offsetType = offsetType is DimensionOffsetType.Bottom ? DimensionOffsetType.Top : DimensionOffsetType.Bottom;
                    var dimensionLineTop = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, offsetType, 0.5);
                    var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["верх", "фронт"]);
                    var dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop,
                                                                  ViewModel.SelectedDimensionType);
                    dimensionTop.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
                }
            }

            var dimensionLineBottomEdge = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, offsetType, 1);
            var refArrayBottomEdge = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                           ["низ", "фронт", "край"]);
            var dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge, 
                                                              ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            var dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(pylon, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            var refArrayRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                          ["низ", "торец", "край"]);
            var dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                     refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }
}
