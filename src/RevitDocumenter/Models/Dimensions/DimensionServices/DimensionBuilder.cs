using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Dimensions.DimensionLines;
using RevitDocumenter.Models.Dimensions.DimensionReferences;
using RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
using RevitDocumenter.Models.Mapping.MapServices;

namespace RevitDocumenter.Models.Dimensions.DimensionServices;
internal class DimensionBuilder {
    private readonly DimensionCreator _dimensionCreator;
    private readonly DimensionChanger _dimensionChanger;
    private readonly ValueGuard _guard;
    private readonly IReferenceCollector<ReferenceToGridsCollectorContext> _comparisonService;
    private readonly IDimensionLineProvider<RebarElementDimensionLineProviderContext> _dimensionLineProvider;
    private readonly ReferenceAnalizeService _referenceAnalizeService;

    public DimensionBuilder(
        DimensionCreator dimensionCreator,
        DimensionChanger dimensionChanger,
        ValueGuard guard,
        IReferenceCollector<ReferenceToGridsCollectorContext> comparisonService,
        IDimensionLineProvider<RebarElementDimensionLineProviderContext> dimensionLineProvider,
        ReferenceAnalizeService referenceAnalizeService) {
        _dimensionCreator = dimensionCreator;
        _dimensionChanger = dimensionChanger;
        _guard = guard;
        _comparisonService = comparisonService;
        _dimensionLineProvider = dimensionLineProvider;
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
        var dimensionRefs = _comparisonService.CollectReferences(
            new ReferenceToGridsCollectorContext(rebarReferences, grids, direction));
        if(dimensionRefs is null) {
            return;
        }

        // Если размер между объектами уже существует, то не ставим - это нормально
        if(_referenceAnalizeService.IsReferenceArrayInList(
            dimensionRefs,
            _referenceAnalizeService.DimensionReferences())) {
            return;
        }

        // Получаем линию размещения размера
        var dimensionLine = _dimensionLineProvider.GetDimensionLine(
            new RebarElementDimensionLineProviderContext(rebar, direction));

        // Строим размер
        var dimension = _dimensionCreator.Create(dimensionLine, dimensionRefs, selectedDimensionType);

        // Если запросили точное расположение размеров
        if(mapInfo != null) {
            // Меняем положение размера в соответствии с картой
            _dimensionChanger.Change(dimension, mapInfo, dimensionRefs);
        }
    }
}
