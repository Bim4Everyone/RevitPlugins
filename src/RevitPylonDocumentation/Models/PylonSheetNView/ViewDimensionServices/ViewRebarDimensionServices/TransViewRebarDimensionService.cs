using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    internal TransViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                            PylonView pylonView, DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
    }


    internal void TryCreateTransViewRebarDimensions(bool onTopOfRebar) {
        try {
            var view = _viewOfPylon.ViewElement;
            var rebarFinder = _viewModel.RebarFinder;
            var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = _sheetInfo.HostElems.First();
            var dimensionLineHostRef = onTopOfRebar ? skeletonParentRebar : pylon;
            // Получаем ссылки на опорные плоскости для размеров бутылок
            var edgeBottomFrontRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                                ["низ", "фронт", "край"]);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            if(onTopOfRebar) {
                if(_sheetInfo.RebarInfo.AllRebarAreL) {
                    // Когда все Гэшки
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["низ", "фронт"]);
                    // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                    ["низ", "фронт", "край"]);
                } else if(_sheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)) {
                        // Верхняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["низ", "фронт"]);
                        // Размер по ФРОНТУ каркас края (положение сверху дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 1,
                                        ["низ", "фронт", "край"]);

                        // Нижняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["верх", "фронт"], edgeBottomFrontRefArray);
                        // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                        ["верх", "фронт", "край"]);
                    } else {
                        // Верхняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["верх", "фронт"], edgeBottomFrontRefArray);
                        // Размер по ФРОНТУ каркас края (положение сверху дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 1,
                                        ["верх", "фронт", "край"]);

                        // Нижняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["низ", "фронт"]);
                        // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                        ["низ", "фронт", "край"]);
                    }
                } else {
                    // Размер по ФРОНТУ каркас (положение снизу ближнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["верх", "фронт"], edgeBottomFrontRefArray);
                    // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                    ["низ", "фронт", "край"]);
                }
            } else {
                // Размер по ФРОНТУ каркас (положение снизу ближнее)
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                ["низ", "фронт"]);
                // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                ["низ", "фронт", "край"]);
            }

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Отступ размерной линии для размера по опалубке
            var formDimensionLineOffset = 0.5;
            // Размер по ТОРЦУ армирование + по бутылкам (положение справа ближнее)
            if(onTopOfRebar && !_sheetInfo.RebarInfo.AllRebarAreL) {
                // Получаем ссылки на опорные плоскости для размеров бутылок
                var edgeBottomSideRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                                   ["низ", "торец", "край"]);
                if(_sheetInfo.RebarInfo.HasLRebar) {
                    if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)
                        && _sheetInfo.RebarInfo.SecondLRebarParamValue) {
                        edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                  ["1", "верх", "торец", "край"],
                                                                  oldRefArray: edgeBottomSideRefArray);
                    } else {
                        edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                  ["2", "верх", "торец", "край"],
                                                                  oldRefArray: edgeBottomSideRefArray);
                    }
                } else {
                    edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                  ["верх", "торец", "край"],
                                                                  oldRefArray: edgeBottomSideRefArray);
                }
                CreateDimension(skeletonParentRebar, pylon, DirectionType.Right, 0.5, edgeBottomSideRefArray,  
                                false);
                // Т.к. мы поставили размер опалубка + армирование, поэтому размер только по опалубке будет стоять дальше
                formDimensionLineOffset = 1;
            }

            // Размер по ТОРЦУ армирование (положение справа дальнее)
            CreateDimension(skeletonParentRebar, pylon, DirectionType.Right, formDimensionLineOffset, 
                            ["низ", "торец", "край"]);
        } catch(Exception) { }
    }

    private void CreateDimension(Element dimensioningElement, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient,
                                 List<string> importantRefNameParts, ReferenceArray oldRefArray = null,
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        ReferenceArray refArray = _dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/',
                                                                        importantRefNameParts, oldRefArray: oldRefArray);
        var dimension = _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, refArray,
                                                                _viewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }

    private void CreateDimension(Element dimensioningElement, Element elemForOffset, DirectionType directionType, 
                                 double offsetCoefficient, ReferenceArray refArray, 
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null) { return; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        var dimension = _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, refArray,
                                                                _viewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }
}
