using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class GeneralViewRebarPerpDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;

    internal GeneralViewRebarPerpDimensionService(MainViewModel mvm, RevitRepository repository, 
                                                  PylonSheetInfo pylonSheetInfo, PylonView pylonView, 
                                                  DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _viewPointsAnalyzer = new ViewPointsAnalyzer(_viewOfPylon);
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
            _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineTop, refArrayTop, 
                                                    _viewModel.SelectedDimensionType);
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
            _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineBottom, refArrayBottom, 
                                                    _viewModel.SelectedDimensionType);
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
                && _viewModel.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection, 
                                                             DirectionType.Left)
                && _sheetInfo.RebarInfo.FirstLRebarParamValue) {
                directionType = DirectionType.Left;
            }
            var dimensionLine = _dimensionBaseService.GetDimensionLine(skeletonParentRebar, directionType, 0.7);
            _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine,
                                                     refArraySide, _viewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размера сверху по Г-образному стержню
    /// </summary>
    internal void TryCreateHorizLRebarDimension(FamilyInstance skeletonParentRebar) {
        try {
            if(!_sheetInfo.RebarInfo.HasLRebar) { return; }
            // Г-образный стержень
            var lRebar = _viewModel.RebarFinder.GetSimpleRebars(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection, 1101)
                                               .FirstOrDefault();
            var dimensionLine = _dimensionBaseService.GetDimensionLine(lRebar, DirectionType.Top, 0.5);
            //"#1_торец_Г_нутрь"
            //"#1_торец_Г_край"
            if(_sheetInfo.RebarInfo.AllRebarAreL) {
                CreateLRebarDimension(skeletonParentRebar, dimensionLine, ["1_торец", "Г"]);
                CreateLRebarDimension(skeletonParentRebar, dimensionLine, ["2_торец", "Г"]);
            } else if(_sheetInfo.RebarInfo.HasLRebar) {
                if(_viewModel.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection, 
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
        _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, refArray,
                                                 _viewModel.SelectedDimensionType);
    }

    /// <summary>
    /// Создание размеров по изгибам вертикальных стержней-бутылок
    /// </summary>
    internal void TryCreateAdditionalDimensions(FamilyInstance skeletonParentRebar) {
        try {
            var doc = _repository.Document;
            var view = _viewOfPylon.ViewElement;
            // Создаем размеры по изгибам бутылок
            var dimensionLineTop = _dimensionBaseService.GetDimensionLine(skeletonParentRebar, DirectionType.Top, -2.3);

            var refArrayTop1 = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,  ["1_торец"], ["Г"]);
            var dimensionTop1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop1,
                                                        _viewModel.SelectedDimensionType);

            var refArrayTop2 = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,  ["2_торец"], ["Г"]);
            var dimensionTop2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop2,
                                                        _viewModel.SelectedDimensionType);
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
