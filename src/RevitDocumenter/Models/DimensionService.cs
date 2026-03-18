using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Comparision;
using RevitDocumenter.Models.DimensionLine;
using RevitDocumenter.Models.MapServices;

namespace RevitDocumenter.Models;
internal class DimensionService {
    private readonly DimensionCreator _dimensionCreator;
    private readonly DimensionChanger _dimensionChanger;
    private readonly ValueGuard _guard;
    private readonly IComparisonService _comparisonService;
    private readonly DimensionLineService _dimensionLineService;
    private readonly ViewMapService _mapService;

    public DimensionService(
        DimensionCreator dimensionCreator,
        DimensionChanger dimensionChanger,
        ValueGuard guard,
        IComparisonService comparisonService,
        DimensionLineService dimensionLineService,
        ViewMapService mapService) {
        _dimensionCreator = dimensionCreator;
        _dimensionChanger = dimensionChanger;
        _guard = guard;
        _comparisonService = comparisonService;
        _dimensionLineService = dimensionLineService;
        _mapService = mapService;
    }

    internal void Create(
        List<RebarElement> rebars,
        List<Grid> grids,
        DimensionType selectedDimensionType,
        bool placeDimensionsAccurately) {
        foreach(var rebar in rebars) {
            // Создание вертикального размера (относительно локальных осей зоны армирования)
            CreateDimension(grids, rebar, selectedDimensionType, placeDimensionsAccurately);
            // Создание горизонтального размера (относительно локальных осей зоны армирования)
            CreateDimension(grids, rebar, selectedDimensionType, placeDimensionsAccurately, false);
        }
    }

    private void CreateDimension(
        List<Grid> grids,
        RebarElement rebar,
        DimensionType selectedDimensionType,
        bool placeDimensionsAccurately,
        bool isForVertical = true) {

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
        // Получаем линию размещения размера
        var dimensionLineY = _dimensionLineService.GetDimensionLine(rebar, direction);

        // Строим размер
        var dimension = _dimensionCreator.Create(dimensionLineY, dimensionRefs, selectedDimensionType);

        // Если запросили точное расположение размеров
        if(placeDimensionsAccurately) {
            // Меняем положение размера в соответствии с картой
            _dimensionChanger.Change(dimension, _mapService, _mapService.ExportOption.MappingStepInFeet, dimensionRefs);
        }
    }
}
