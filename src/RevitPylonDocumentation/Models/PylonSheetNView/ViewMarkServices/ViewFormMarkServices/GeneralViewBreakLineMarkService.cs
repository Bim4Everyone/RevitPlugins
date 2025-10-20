using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;

internal class GeneralViewBreakLineMarkService {
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly FamilySymbol _breakLineSymbol;
    private readonly BreakLinePointsService _breakLinePointsService;
    private readonly BreakLineParameterService _breakLineParameterService;

    // Отступы для формирования линий обрыва
    private readonly double _breakLinesOffsetX = 0.5;
    private readonly double _breakLinesOffsetY = 0.3;
    private readonly double _breakLinesOffsetYBottom = 1;

    internal GeneralViewBreakLineMarkService(CreationSettings settings, Document document, 
                                             PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        var viewPointsAnalyzer = new ViewPointsAnalyzerService(_viewOfPylon);
        var floorAnalyzerService = new FloorAnalyzerService(document, pylonSheetInfo);

        _breakLinePointsService = new BreakLinePointsService(
            viewPointsAnalyzer,
            floorAnalyzerService,
            pylonSheetInfo,
            _breakLinesOffsetX,
            _breakLinesOffsetY,
            _breakLinesOffsetYBottom);

        _breakLineParameterService = new BreakLineParameterService();
        _breakLineSymbol = settings.TypesSettings.SelectedBreakLineType;
    }


    /// <summary>
    /// Создаёт линии обрыва ниже первого опалубочного элемента
    /// </summary>
    internal void TryCreateLowerBreakLines(bool isForPerpView) {
        if(_breakLineSymbol is null)
            return;

        var view = _viewOfPylon.ViewElement;
        try {
            var points = _breakLinePointsService.GetBreakLinePointsForLowerLines(view, isForPerpView);
            var lines = CreateLinesFromPoints(points, true);
            CreateBreakLine(lines, view);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаёт линии обрыва выше последнего опалубочного элемента
    /// </summary>
    internal void TryCreateUpperBreakLines() {
        if(_breakLineSymbol is null)
            return;

        var view = _viewOfPylon.ViewElement;
        try {
            var points = _breakLinePointsService.GetBreakLinePointsForUpperLines(view);
            var lines = CreateLinesFromPoints(points, false);
            CreateBreakLine(lines, view);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаёт линии обрыва между опалубочными элементами
    /// </summary>
    internal void TryCreateMiddleBreakLines(bool isForPerpView) {
        if(_sheetInfo.HostElems.Count == 1 || _breakLineSymbol is null)
            return;

        var view = _viewOfPylon.ViewElement;
        try {
            var points = _breakLinePointsService.GetBreakLinePointsForMiddleLines(view, isForPerpView);
            var lines = CreateLinesFromPoints(points, false);
            CreateBreakLine(lines, view);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаёт линии обрыва на основе списка точек
    /// </summary>
    /// <param name="points">Список точек, из которых будут созданы линии.</param>
    /// <param name="isLowerZone">Флаг, указывающий, что точки относятся к нижней зоне (3 линии).</param>
    /// <returns>Список линий обрыва.</returns>
    private List<Line> CreateLinesFromPoints(List<XYZ> points, bool isLowerZone) {
        if(points.Count < 4) {
            throw new ArgumentException("Ожидается 4 точки для создания линий обрыва.");
        }

        var lines = new List<Line>();
        if(isLowerZone) {
            // Нижняя зона: 3 линии (1-2, 2-3, 3-4)
            lines.Add(Line.CreateBound(points[0], points[1]));
            lines.Add(Line.CreateBound(points[1], points[2]));
            lines.Add(Line.CreateBound(points[2], points[3]));
        } else {
            // Верхняя/средняя зона: 2 линии (1-2, 3-4)
            lines.Add(Line.CreateBound(points[0], points[1]));
            lines.Add(Line.CreateBound(points[2], points[3]));
        }
        return lines;
    }


    /// <summary>
    /// Создаёт экземпляры линий обрыва в Revit
    /// </summary>
    private void CreateBreakLine(List<Line> lines, View view) {
        if(_breakLineSymbol is null) {
            throw new InvalidOperationException("Символ линии обрыва не задан.");
        }
        
        foreach(var line in lines) {
            try {
                var breakLine = _doc.Create.NewFamilyInstance(line, _breakLineSymbol, view);
                _breakLineParameterService.TrySetBreakLineOffsets(breakLine);
            } catch(Exception) { }
        }
    }
}
