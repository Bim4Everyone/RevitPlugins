using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Comparision;
using RevitDocumenter.Models.MapServices;

namespace RevitDocumenter.Models.DimensionServices;
internal class DimensionService {
    private readonly RevitRepository _revitRepository;
    private readonly DimensionCreator _dimensionCreator;
    private readonly DimensionChanger _dimensionChanger;
    private readonly ValueGuard _guard;
    private readonly IComparisonService _comparisonService;
    private readonly DimensionLineService _dimensionLineService;
    private readonly ReferenceAnalizeService _referenceAnalizeService;

    public DimensionService(
        RevitRepository revitRepository,
        DimensionCreator dimensionCreator,
        DimensionChanger dimensionChanger,
        ValueGuard guard,
        IComparisonService comparisonService,
        DimensionLineService dimensionLineService,
        ReferenceAnalizeService referenceAnalizeService) {
        _revitRepository = revitRepository;
        _dimensionCreator = dimensionCreator;
        _dimensionChanger = dimensionChanger;
        _guard = guard;
        _comparisonService = comparisonService;
        _dimensionLineService = dimensionLineService;
        _referenceAnalizeService = referenceAnalizeService;
    }

    internal void Create(
        List<RebarElement> rebars,
        List<Grid> grids,
        DimensionType selectedDimensionType,
        MapInfo mapInfo) {
        foreach(var rebar in rebars) {
            // Создание вертикального размера (относительно локальных осей зоны армирования)
            CreateDimension(grids, rebar, selectedDimensionType, true, mapInfo);
            // Создание горизонтального размера (относительно локальных осей зоны армирования)
            CreateDimension(grids, rebar, selectedDimensionType, false, mapInfo);
        }
    }

    private void CreateDimension(
        List<Grid> grids,
        RebarElement rebar,
        DimensionType selectedDimensionType,
        bool isForVertical,
        MapInfo mapInfo) {

        try {
            _guard.ThrowIfNull(grids, rebar);
        } catch(Exception) {
            return;
        }

        var rebarReferences = isForVertical ? rebar.VerticalRefs : rebar.HorizontalRefs;
        // Нормальная ситуация, когда в зоне армирования какие-то опорные плоскости не были найдены
        if(rebarReferences.Count == 0)
            return;
        var direction = isForVertical ? rebar.Rebar.FacingOrientation : rebar.Rebar.HandOrientation;


        // Получаем опорные плоскости для размера
        // Нормальная ситуация, когда подходящие оси не были найдены
        var dimensionRefs = _comparisonService.Compare(new GridComparisonContext(rebarReferences, grids, direction));
        if(dimensionRefs is null) {
            return;
        }

        // Если размер между объектами уже существует, то не ставим - это нормально
        if(_referenceAnalizeService.IsReferenceArrayInList(
            dimensionRefs,
            _revitRepository.DimensionReferences())) {
            return;
        }

        // Получаем линию размещения размера
        var dimensionLine = _dimensionLineService.GetDimensionLine(rebar, direction);

        // Строим размер
        var dimension = _dimensionCreator.Create(dimensionLine, dimensionRefs, selectedDimensionType);

        // Если запросили точное расположение размеров
        if(mapInfo != null) {
            // Меняем положение размера в соответствии с картой
            _dimensionChanger.Change(dimension, mapInfo, dimensionRefs);
        }
    }
}
