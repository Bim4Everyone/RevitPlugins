using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarDimensionService {
    private readonly DimensionBaseService _dimensionBaseService;
    internal TransViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                            DimensionBaseService dimensionBaseService) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        _dimensionBaseService = dimensionBaseService;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    internal void TryCreateTransViewRebarDimensions(View view, bool onTopOfRebar) {
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = SheetInfo.HostElems.First();
            var dimensionLineHostRef = onTopOfRebar ? skeletonParentRebar : pylon;
            // Получаем ссылки на опорные плоскости для размеров бутылок
            var edgeBottomFrontRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                                ["низ", "фронт", "край"]);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            if(onTopOfRebar) {
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    // Когда все Гэшки
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["низ", "фронт"], view);
                    // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                    ["низ", "фронт", "край"], view);
                } else if(SheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, SheetInfo.ProjectSection, DirectionType.Top)) {
                        // Верхняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["низ", "фронт"], view);
                        // Размер по ФРОНТУ каркас края (положение сверху дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 1,
                                        ["низ", "фронт", "край"], view);

                        // Нижняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["верх", "фронт"], view, edgeBottomFrontRefArray);
                        // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                        ["верх", "фронт", "край"], view);
                    } else {
                        // Верхняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["верх", "фронт"], view, edgeBottomFrontRefArray);
                        // Размер по ФРОНТУ каркас края (положение сверху дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 1,
                                        ["верх", "фронт", "край"], view);

                        // Нижняя зона
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["низ", "фронт"], view);
                        // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                        ["низ", "фронт", "край"], view);
                    }
                } else {
                    // Размер по ФРОНТУ каркас (положение снизу ближнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["верх", "фронт"], view, edgeBottomFrontRefArray);
                    // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                    ["низ", "фронт", "край"], view);
                }
            } else {
                // Размер по ФРОНТУ каркас (положение снизу ближнее)
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                ["низ", "фронт"], view);
                // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                                ["низ", "фронт", "край"], view);
            }

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Отступ размерной линии для размера по опалубке
            var formDimensionLineOffset = 0.5;
            // Размер по ТОРЦУ армирование + по бутылкам (положение справа ближнее)
            if(onTopOfRebar && !SheetInfo.RebarInfo.AllRebarAreL) {
                // Получаем ссылки на опорные плоскости для размеров бутылок
                var edgeBottomSideRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                                   ["низ", "торец", "край"]);
                if(SheetInfo.RebarInfo.HasLRebar) {
                    if(rebarFinder.DirectionHasLRebar(view, SheetInfo.ProjectSection, DirectionType.Top)
                        && SheetInfo.RebarInfo.SecondLRebarParamValue) {
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
                CreateDimension(skeletonParentRebar, pylon, DirectionType.Right, 0.5, edgeBottomSideRefArray, view, 
                                false);
                // Т.к. мы поставили размер опалубка + армирование, поэтому размер только по опалубке будет стоять дальше
                formDimensionLineOffset = 1;
            }

            // Размер по ТОРЦУ армирование (положение справа дальнее)
            CreateDimension(skeletonParentRebar, pylon, DirectionType.Right, formDimensionLineOffset, 
                            ["низ", "торец", "край"], view);
        } catch(Exception) { }
    }

    private void CreateDimension(Element dimensioningElement, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient,
                                 List<string> importantRefNameParts, View view, ReferenceArray oldRefArray = null,
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        ReferenceArray refArray = _dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/',
                                                                        importantRefNameParts, oldRefArray: oldRefArray);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, refArray,
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }

    private void CreateDimension(Element dimensioningElement, Element elemForOffset, DirectionType directionType, 
                                 double offsetCoefficient, ReferenceArray refArray, View view, 
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null) { return; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, refArray,
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }
}
