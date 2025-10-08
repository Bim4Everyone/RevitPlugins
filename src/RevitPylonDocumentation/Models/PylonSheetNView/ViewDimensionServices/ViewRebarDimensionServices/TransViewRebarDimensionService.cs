using System;
using System.Linq;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionCreationService _dimCreationService;

    internal TransViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                            PylonView pylonView, DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimCreationService = new DimensionCreationService(mvm, repository, pylonView, _dimensionBaseService);
    }


    internal void TryCreateTransViewRebarDimensions(bool onTopOfRebar, bool dimensionLineFromPylon) {
        try {
            var view = _viewOfPylon.ViewElement;
            var rebarFinder = _viewModel.RebarFinder;
            var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = _sheetInfo.HostElems.First();
            var dimensionLineHostRef = dimensionLineFromPylon ? pylon : skeletonParentRebar;
            // Получаем ссылки на опорные плоскости для размеров бутылок
            var edgeBottomFrontRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                                 ["низ", "фронт", "край"]);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            var topSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Top, 0.5);
            var topBigOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Top, 1);
            var bottomSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom, 0.5);
            var bottomBigOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom, 1);
            
            if(onTopOfRebar) {
                if(_sheetInfo.RebarInfo.AllRebarAreL) {
                    // Когда все Гэшки
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["низ", "фронт"]);
                    // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomBigOffset, ["низ", "фронт", "край"]);
                } else if(_sheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)) {
                        // Верхняя зона
                        _dimCreationService.CreateDimension(skeletonParentRebar, topSmallOffset, ["низ", "фронт"]);
                        // Размер по ФРОНТУ каркас края (положение сверху дальнее)
                        _dimCreationService.CreateDimension(skeletonParentRebar, topBigOffset, 
                                                            ["низ", "фронт", "край"]);

                        // Нижняя зона
                        _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["верх", "фронт"], 
                                                            edgeBottomFrontRefArray);
                        // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                        _dimCreationService.CreateDimension(skeletonParentRebar, bottomBigOffset, 
                                                            ["верх", "фронт", "край"]);
                    } else {
                        // Верхняя зона
                        _dimCreationService.CreateDimension(skeletonParentRebar, topSmallOffset, ["верх", "фронт"], 
                                                            edgeBottomFrontRefArray);
                        // Размер по ФРОНТУ каркас края (положение сверху дальнее)
                        _dimCreationService.CreateDimension(skeletonParentRebar, topBigOffset, 
                                                            ["верх", "фронт", "край"]);

                        // Нижняя зона
                        _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["низ", "фронт"]);
                        // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                        _dimCreationService.CreateDimension(skeletonParentRebar, bottomBigOffset, 
                                                            ["низ", "фронт", "край"]);
                    }
                } else {
                    // Размер по ФРОНТУ каркас (положение снизу ближнее)
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["верх", "фронт"], 
                                                        edgeBottomFrontRefArray);
                    // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomBigOffset, ["низ", "фронт", "край"]);
                }
            } else {
                // Размер по ФРОНТУ каркас (положение снизу ближнее)
                _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["низ", "фронт"]);
                // Размер по ФРОНТУ каркас края (положение снизу дальнее)
                _dimCreationService.CreateDimension(skeletonParentRebar, bottomBigOffset, ["низ", "фронт", "край"]);
            }

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Отступ размерной линии для размера по опалубке
            var formDimensionLineOffset = 0.5;
            // Размер по ТОРЦУ армирование + по бутылкам (положение справа ближнее)
            if(onTopOfRebar && !_sheetInfo.RebarInfo.AllRebarAreL) {
                // Получаем ссылки на опорные плоскости для размеров бутылок
                var edgeBottomSideRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                                    ["низ", "торец", "край"]);
                if(_sheetInfo.RebarInfo.HasLRebar) {
                    if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)
                        && _sheetInfo.RebarInfo.SecondLRebarParamValue) {
                        edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                   ["1", "верх", "торец", "край"],
                                                                   oldRefArray: edgeBottomSideRefArray);
                    } else {
                        edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                   ["2", "верх", "торец", "край"],
                                                                   oldRefArray: edgeBottomSideRefArray);
                    }
                } else {
                    edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                   ["верх", "торец", "край"],
                                                                   oldRefArray: edgeBottomSideRefArray);
                }

                var edgeBottomSideOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, 0.5);
                _dimCreationService.CreateDimension(edgeBottomSideRefArray, edgeBottomSideOffset, false);

                // Т.к. мы поставили размер опалубка + армирование, поэтому размер только по опалубке будет стоять дальше
                formDimensionLineOffset = 1;
            }

            // Размер по ТОРЦУ армирование (положение справа дальнее)
            var rebarSideOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, formDimensionLineOffset);
            _dimCreationService.CreateDimension(skeletonParentRebar, rebarSideOffset, ["низ", "торец", "край"]);
        } catch(Exception) { }
    }
}
