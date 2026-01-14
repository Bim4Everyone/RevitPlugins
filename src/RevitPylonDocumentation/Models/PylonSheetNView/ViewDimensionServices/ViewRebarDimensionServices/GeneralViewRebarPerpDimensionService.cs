using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class GeneralViewRebarPerpDimensionService {
    private readonly DimensionType _selectedDimensionType;
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly ViewPointsAnalyzerService _viewPointsAnalyzer;

    internal GeneralViewRebarPerpDimensionService(CreationSettings settings, Document document,
                                                  PylonSheetInfo pylonSheetInfo, PylonView pylonView,
                                                  DimensionBaseService dimensionBaseService) {
        _selectedDimensionType = settings.ProjectSettings.SelectedDimensionType;
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _viewPointsAnalyzer = new ViewPointsAnalyzerService(_viewOfPylon);
    }


    /// <summary>
    /// Создание размерной цепочки по крайним вертикальным стержням арматурного каркаса сверху
    /// </summary>
    internal void TryCreateTopEdgeRebarDimensions(FamilyInstance skeletonParentRebar) {
        try {
            // Если хотя бы один из стержней - Г-образный,тогда нет смысле ставить этот размер
            // Ставится только когда обе бутылки
            if(_sheetInfo.RebarInfo.HasLRebar) { return; }
            var dimensionLineTop = _dimensionBaseService.GetDimensionLine(skeletonParentRebar, DirectionType.Top, 1);
            var refArrayTop = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, ["верх", "торец"]);
            _doc.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineTop, refArrayTop,
                                                     _selectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размерной цепочки по крайним вертикальным стержням арматурного каркаса снизу
    /// </summary>
    internal void TryCreateBottomEdgeRebarDimensions(FamilyInstance skeletonParentRebar) {
        try {
            var dimensionLineBottom = _dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                            DirectionType.Bottom, 0.6);
            var refArrayBottom = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, ["низ", "торец"]);
            _doc.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineBottom, refArrayBottom,
                                                     _selectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размера сбоку между низом арматурного каркаса и Г-образным стержнем
    /// </summary>
    internal void TryCreateVertLRebarDimension(FamilyInstance skeletonParentRebar) {
        try {
            if(!_sheetInfo.RebarInfo.HasLRebar) { return; }
            var refArraySideBottom = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                            ["горизонт", "край", "низ"]);
            var refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                      ["горизонт", "Г-стержень"],
                                                                      oldRefArray: refArraySideBottom);
            var directionType = DirectionType.Right;
            // Если Гэшка только слева и включен первая Гэшка в семействе
            if(!_sheetInfo.RebarInfo.AllRebarAreL
                && _sheetInfo.RebarInfo.HasLRebar
                && _sheetInfo.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection,
                                                             DirectionType.Left)
                && _sheetInfo.RebarInfo.FirstLRebarParamValue) {
                directionType = DirectionType.Left;
            }
            var dimensionLine = _dimensionBaseService.GetDimensionLine(skeletonParentRebar, directionType, 0.7);
            _doc.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine,
                                                     refArraySide, _selectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размера сверху по Г-образному стержню
    /// </summary>
    internal void TryCreateHorizLRebarDimension(FamilyInstance skeletonParentRebar) {
        try {
            if(!_sheetInfo.RebarInfo.HasLRebar) { return; }
            // Г-образный стержень
            var lRebar = _sheetInfo.RebarFinder.GetSimpleRebars(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection, 1101)
                                               .FirstOrDefault();
            var dimensionLine = _dimensionBaseService.GetDimensionLine(lRebar, DirectionType.Top, 0.5);
            //"#1_торец_Г_нутрь"
            //"#1_торец_Г_край"
            if(_sheetInfo.RebarInfo.AllRebarAreL) {
                CreateLRebarDimension(skeletonParentRebar, dimensionLine, ["1_торец", "Г"]);
                CreateLRebarDimension(skeletonParentRebar, dimensionLine, ["2_торец", "Г"]);
            } else if(_sheetInfo.RebarInfo.HasLRebar) {
                if(_sheetInfo.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection,
                                                             DirectionType.Right)
                    && _sheetInfo.RebarInfo.SecondLRebarParamValue) {
                    CreateLRebarDimension(skeletonParentRebar, dimensionLine, ["2_торец", "Г"]);
                } else {
                    CreateLRebarDimension(skeletonParentRebar, dimensionLine, ["1_торец", "Г"]);
                }
            }
        } catch(Exception) { }
    }

    private void CreateLRebarDimension(FamilyInstance skeletonParentRebar, Line dimensionLine,
                                       List<string> importantRefNameParts,
                                       List<string> unimportantRefNameParts = null) {
        var refArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                              importantRefNameParts,
                                                              unimportantRefNameParts);
        _doc.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, refArray,
                                                 _selectedDimensionType);
    }

    /// <summary>
    /// Создание размеров по изгибам вертикальных стержней-бутылок
    /// </summary>
    internal void TryCreateAdditionalDimensions(FamilyInstance skeletonParentRebar) {
        try {
            var doc = _doc;
            var view = _viewOfPylon.ViewElement;
            // Создаем размеры по изгибам бутылок
            var dimensionLineTop = _dimensionBaseService.GetDimensionLine(skeletonParentRebar, DirectionType.Top, -2.3);

            var refArrayTop1 = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, ["1_торец"], ["Г"]);
            var dimensionTop1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop1,
                                                        _selectedDimensionType);

            var refArrayTop2 = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, ["2_торец"], ["Г"]);
            var dimensionTop2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop2,
                                                        _selectedDimensionType);
            // Смещаем текст размера для корректного отображения
            var rightDirection = view.RightDirection;
            var rightDirectionInversed = rightDirection.Negate();
            double offsetX = 0.3;

            var offsetLeft = new XYZ(rightDirection.X * offsetX, rightDirection.Y * offsetX, rightDirection.Z);
            var offsetRight = new XYZ(rightDirectionInversed.X * offsetX,
                                      rightDirectionInversed.Y * offsetX,
                                      rightDirectionInversed.Z);
            var textPosition1 = dimensionTop1.TextPosition;
            var textPosition1Transformed = _viewPointsAnalyzer.GetTransformedPoint(textPosition1);

            var textPosition2 = dimensionTop2.TextPosition;
            var textPosition2Transformed = _viewPointsAnalyzer.GetTransformedPoint(textPosition2);
            if(textPosition1Transformed.X > textPosition2Transformed.X) {
                dimensionTop1.TextPosition += offsetLeft;
                dimensionTop2.TextPosition += offsetRight;
            } else {
                dimensionTop1.TextPosition += offsetRight;
                dimensionTop2.TextPosition += offsetLeft;
            }

            // В случае, если размер имеет нулевое значение, то удаляем его
            if(dimensionTop1.ValueString == "0") {
                doc.Delete(dimensionTop1.Id);
                dimensionTop1 = null;
            }
            if(dimensionTop2.ValueString == "0") {
                doc.Delete(dimensionTop2.Id);
                dimensionTop2 = null;
            }
        } catch(Exception) { }
    }
}
