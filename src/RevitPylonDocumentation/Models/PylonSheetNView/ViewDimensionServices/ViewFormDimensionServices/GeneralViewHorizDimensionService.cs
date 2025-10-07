using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class GeneralViewHorizDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionSegmentsService _dimSegmentsService;
    private readonly FloorAnalyzerService _floorAnalyzerService;

    internal GeneralViewHorizDimensionService(MainViewModel mvm, RevitRepository repository, 
                                              PylonSheetInfo pylonSheetInfo, PylonView pylonView, 
                                              DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimSegmentsService = new DimensionSegmentsService(_viewOfPylon.ViewElement);
        _floorAnalyzerService = new FloorAnalyzerService(repository, pylonSheetInfo);
    }

    /// <summary>
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    private void TryCreatePylonDimensions(List<Element> hostElems, bool isForPerpView) {
        try {
            // Если этот размер для перпендикулярного вида и Гэшка только слева, то размер нужно ставить справа
            var dimensionLineDirection = isForPerpView
                                         && !_sheetInfo.RebarInfo.AllRebarAreL
                                         && _sheetInfo.RebarInfo.HasLRebar
                                         && _viewModel.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement,
                                                                                      _sheetInfo.ProjectSection,
                                                                                      DirectionType.Left)
                                         ? DirectionType.Right : DirectionType.Left;

            var dimensionLineLeft = _dimensionBaseService.GetDimensionLine(hostElems.First() as FamilyInstance,
                                                                          dimensionLineDirection, 1.7);
            ReferenceArray refArray = default;
            foreach(var item in hostElems) {
                if(item is not FamilyInstance hostElem) { return; }
                // Собираем опорные плоскости по опалубке, например:
                // #_1_горизонт_край_низ
                refArray = _dimensionBaseService.GetDimensionRefs(hostElem, ["горизонт", "край", "низ"], 
                                                                  oldRefArray: refArray);
            }
            if(refArray != default) {
                var lastFloor = _floorAnalyzerService.GetLastFloor();
                if(lastFloor != null) {
                    var viewOptions = new Options {
                        View = _viewOfPylon.ViewElement,
                        ComputeReferences = true,
                        IncludeNonVisibleObjects = false
                    };
                    var lastFloorTopFace = _floorAnalyzerService.GetTopFloorFace(lastFloor, viewOptions);
                    refArray.Append(lastFloorTopFace.Reference);
                }
                _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineLeft, refArray,
                                                         _viewModel.SelectedDimensionType);
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает размер по выпуску вертикального армирования, если оно есть
    /// </summary>
    private void TryCreateTopAdditionalDimensions(FamilyInstance rebar, bool isForPerpView) {
        try {
            if(_sheetInfo.RebarInfo.AllRebarAreL) {
                return;
            }
            var lastFloor = _floorAnalyzerService.GetLastFloor();
            if(lastFloor is null) {
                return;
            }
            var viewOptions = new Options {
                View = _viewOfPylon.ViewElement,
                ComputeReferences = true,
                IncludeNonVisibleObjects = false
            };
            var lastFloorTopFace = _floorAnalyzerService.GetTopFloorFace(lastFloor, viewOptions);
            var lastFloorBottomFace = _floorAnalyzerService.GetBottomFloorFace(lastFloor, viewOptions);

            // Если этот размер для перпендикулярного вида и Гэшка только слева, то размер нужно ставить справа
            var dimensionLineDirection = isForPerpView
                                         && !_sheetInfo.RebarInfo.AllRebarAreL
                                         && _sheetInfo.RebarInfo.HasLRebar
                                         && _viewModel.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement,
                                                                                      _sheetInfo.ProjectSection,
                                                                                      DirectionType.Left)
                                         ? DirectionType.Right : DirectionType.Left;
            // Определяем размерную линию
            var dimensionLineLeft = _dimensionBaseService.GetDimensionLine(_sheetInfo.HostElems.First() as FamilyInstance,
                                                                           dimensionLineDirection, 
                                                                           1.1);
            // #1_горизонт_выпуск
            var refArray = _dimensionBaseService.GetDimensionRefs(rebar, ["горизонт", "выпуск"]);
            if(refArray.Size == 0) { return; }
            refArray.Append(lastFloorTopFace.Reference);
            refArray.Append(lastFloorBottomFace.Reference);
            _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineLeft, refArray,
                                                     _viewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает размеры между нижней и верхней плоскостью пилона, и нижней и верхней плоскостью хомутов
    /// </summary>
    private void TryCreateClampsDimensions(List<FamilyInstance> clampsParentRebars, bool isForPerpView) {
        try {
            if(clampsParentRebars is null || clampsParentRebars.Count == 0) { return; }
            ReferenceArray refArray = null;
            // #_1_горизонт_край_низ, #_1_горизонт_край_верх
            foreach(var host in _sheetInfo.HostElems) {
                refArray = _dimensionBaseService.GetDimensionRefs(host as FamilyInstance, 
                                                                  ["горизонт", "край"], 
                                                                  oldRefArray: refArray);
            }

            var dimensionLineDirection = DirectionType.Left;
            var textOffset = _dimSegmentsService.HorizSmallUpDirectDimTextOffset;
            var textOffsetInverted = _dimSegmentsService.HorizSmallUpInvertedDimTextOffset;
            // Если этот размер для перпендикулярного вида и Гэшка только справа, то размер нужно ставить слева
            if(isForPerpView && !_sheetInfo.RebarInfo.AllRebarAreL 
                             && _sheetInfo.RebarInfo.HasLRebar
                             && _viewModel.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement,
                                                                          _sheetInfo.ProjectSection,
                                                                          DirectionType.Left)) {
                dimensionLineDirection = DirectionType.Right;
                textOffset = _dimSegmentsService.HorizSmallDownDirectDimText;
                textOffsetInverted = _dimSegmentsService.HorizSmallDownInvertedDimTextOffset;
            }

            // Создаем коллекцию опций изменений будущего размера
            var dimSegmentOpts = new List<DimensionSegmentOption>();

            foreach(var clampsParentRebar in clampsParentRebars) {
                // В связи с особенностями семейства, нельзя задать именем опорной плоскости правила чтобы 
                // забирать именно крайнюю плоскость, т.к. плоскости смещаются и накладываются
                // Всегда отсеивать (они накладываются на #1_горизонт_доборные_низ_край): 
                // #2_горизонт_доборные_низ
                // #5_горизонт_доборные_верх
                bool additionalFirst = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 1") == 1;
                bool additionalFirstArray = clampsParentRebar.GetParamValue<int>("b4e_Доборный 1_Массив") == 1;

                bool additionalSecond = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 2") == 1;
                bool additionalSecondArray = clampsParentRebar.GetParamValue<int>("b4e_Доборный 2_Массив") == 1;
                // #3_горизонт_низ
                // мод_ФОП_Доборный 1#2_горизонт_доборные_низ
                // мод_ФОП_Доборный 1/мод_ФОП_Доборный 1_Массив#1_горизонт_доборные_низ_край
                if(additionalFirst && additionalFirstArray) {
                    refArray = _dimensionBaseService.GetDimensionRefs(clampsParentRebar, 
                                                                      ["горизонт", "доборные", "низ", "край"],
                                                                      oldRefArray: refArray);
                } else if(additionalFirst) {
                    refArray = _dimensionBaseService.GetDimensionRefs(clampsParentRebar, 
                                                                      ["горизонт", "доборные", "низ"], 
                                                                      ["край"],
                                                                      oldRefArray: refArray);
                } else {
                    refArray = _dimensionBaseService.GetDimensionRefs(clampsParentRebar, 
                                                                      ["горизонт", "низ"],
                                                                      ["доборные", "край"], 
                                                                      oldRefArray: refArray);
                }

                if(additionalSecond && additionalSecondArray) {
                    refArray = _dimensionBaseService.GetDimensionRefs(clampsParentRebar, 
                                                                      ["горизонт", "доборные", "верх", "край"],
                                                                      oldRefArray: refArray);
                } else if(additionalSecond) {
                    refArray = _dimensionBaseService.GetDimensionRefs(clampsParentRebar, 
                                                                      ["горизонт", "доборные", "верх"], 
                                                                      ["край"],
                                                                      oldRefArray: refArray);
                } else {
                    refArray = _dimensionBaseService.GetDimensionRefs(clampsParentRebar, 
                                                                      ["горизонт", "верх"],
                                                                      ["доборные", "край"], 
                                                                      oldRefArray: refArray);
                }
                // Добавляем запись про размер низ пилона - низ хомутов
                dimSegmentOpts.Add(new DimensionSegmentOption(true, "", textOffset));
                // Добавляем запись про размер низ хомутов - верх хомутов
                dimSegmentOpts.Add(new DimensionSegmentOption(false));
                // Добавляем запись про размер верх хомутов - верх пилона
                dimSegmentOpts.Add(new DimensionSegmentOption(true, "", textOffsetInverted));
                // Добавляем запись про размер промежуточная плита
                dimSegmentOpts.Add(new DimensionSegmentOption(false));
            }

            // Определяем размерную линию
            var dimensionLineLeft = _dimensionBaseService.GetDimensionLine(_sheetInfo.HostElems.First() as FamilyInstance,
                                                                           dimensionLineDirection, 1.1);
            var dimensionRebarSide =
                _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLineLeft, refArray,
                                                         _viewModel.SelectedDimensionType);

            // Применяем опции изменений сегментов размера
            _dimSegmentsService.ApplySegmentsModification(dimensionRebarSide, dimSegmentOpts);
        } catch(Exception) { }
    }

    internal void CreateDimensions(FamilyInstance skeletonParentRebar, bool isForPerpView) {
        // Получаем родительское семейство хомутов на виде
        var rebarFinder = _viewModel.RebarFinder;
        var clampsParentRebars = rebarFinder.GetClampsParentRebars(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection);
        if(clampsParentRebars is null) { return; }

        // Размер по опалубке пилонов (положение слева/справа 1.7)
        TryCreatePylonDimensions(_sheetInfo.HostElems, isForPerpView);

        // Размер по выпуску вертикального армирования (положение слева/справа 1.1)
        TryCreateTopAdditionalDimensions(skeletonParentRebar, isForPerpView);

        // Размер по крайним хомутам (положение слева/справа 1.1)
        TryCreateClampsDimensions(clampsParentRebars, isForPerpView);
    }
}
