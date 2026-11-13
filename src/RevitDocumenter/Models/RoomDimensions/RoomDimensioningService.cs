using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Diagnostics;
using RevitDocumenter.Models.RoomDimensions.Geometry;
using RevitDocumenter.Models.RoomDimensions.Layout;
using RevitDocumenter.Models.RoomDimensions.Planning;
using RevitDocumenter.Models.RoomDimensions.Settings;

namespace RevitDocumenter.Models.RoomDimensions;

/// <summary>
/// Запускает образмеривание помещений на активном виде.
/// Шаг 2: удаление своих размеров, контурная модель, привязки, пробные размеры и журнал.
/// </summary>
internal class RoomDimensioningService {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly RoomContourBuilder _contourBuilder;
    private readonly ReferenceResolver _referenceResolver;
    private readonly ContourDimensionPlanner _planner;
    private readonly VirtualDimensionBuilder _dimensionBuilder;
    private readonly DebugDumpWriter _dumpWriter;
    private readonly ContourDebugDrawer _contourDrawer;
    private readonly ViewImageExporter _imageExporter;

    public RoomDimensioningService(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        RoomContourBuilder contourBuilder,
        ReferenceResolver referenceResolver,
        ContourDimensionPlanner planner,
        VirtualDimensionBuilder dimensionBuilder,
        DebugDumpWriter dumpWriter,
        ContourDebugDrawer contourDrawer,
        ViewImageExporter imageExporter) {
        _revitRepository = revitRepository.ThrowIfNull();
        _localizationService = localizationService.ThrowIfNull();
        _contourBuilder = contourBuilder.ThrowIfNull();
        _referenceResolver = referenceResolver.ThrowIfNull();
        _planner = planner.ThrowIfNull();
        _dimensionBuilder = dimensionBuilder.ThrowIfNull();
        _dumpWriter = dumpWriter.ThrowIfNull();
        _contourDrawer = contourDrawer.ThrowIfNull();
        _imageExporter = imageExporter.ThrowIfNull();
    }

    public RoomDimensioningResult Run(RoomDimensioningSettings settings, string settingsPath) {
        settings.ThrowIfNull();
        var result = new RoomDimensioningResult();

        var dimensionTypes = _revitRepository.DimensionTypes
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());
        var pluginTypeNames = new HashSet<string>(settings.RoomTypes.Select(t => t.DimensionTypeName));

        // Значения раскладки: мм на печати * масштаб вида -> мм модели -> футы
        int scale = _revitRepository.ActiveView.Scale;
        double offset = UnitUtilsHelper.ConvertToInternalValue(settings.Layout.FirstTierOffsetPrintMm * scale);
        double maxWitnessLine = UnitUtilsHelper.ConvertToInternalValue(settings.Layout.MaxWitnessLinePrintMm * scale);
        double tierStep = UnitUtilsHelper.ConvertToInternalValue(settings.Layout.TierStepPrintMm * scale);

        var warningSwallower = new WarningSwallower();
        using(var transaction = new Transaction(
                  _revitRepository.Document,
                  _localizationService.GetLocalizedString("RoomDimensioningWindow.Title"))) {
            var failureOptions = transaction.GetFailureHandlingOptions();
            failureOptions.SetFailuresPreprocessor(warningSwallower);
            transaction.SetFailureHandlingOptions(failureOptions);
            transaction.Start();

            result.DeletedDimensionCount = _revitRepository.DeleteDimensionsOfTypes(pluginTypeNames);
            result.DoorObstacles = new DoorObstacleCollector(
                _revitRepository.Document, _revitRepository.ActiveView).Collect();
            if(settings.Debug.Enabled) {
                CollectViewDiagnostics(result);
            }

            foreach(var room in _revitRepository.GetRoomsOnActiveView()) {
                var roomType = RoomTypeMatcher.Match(room, settings);
                if(roomType is null) {
                    result.UnmatchedRoomCount++;
                    continue;
                }

                var contour = _contourBuilder.Build(room, roomType);
                var roomResult = new RoomRunResult(contour);
                result.Rooms.Add(roomResult);
                if(contour.Error != null) {
                    continue;
                }

                _referenceResolver.Resolve(contour);
                if(dimensionTypes.TryGetValue(roomType.DimensionTypeName, out var dimensionType)) {
                    double textSize = DimensionTextHelper.GetTextSize(dimensionType, scale);
                    var options = new PlanningOptions {
                        Offset = offset,
                        TextAllowance = DimensionTextHelper.GetTextAllowance(dimensionType, scale),
                        TextSize = textSize,
                        TextGap = DimensionTextHelper.GetTextGap(dimensionType, scale),
                        TextWidthScale = DimensionTextHelper.GetTextWidthScale(dimensionType),
                        // Плоскости и значения сравниваются с допуском типа помещения, но не точнее 0.5 мм
                        CoordinateTolerance = UnitUtilsHelper.ConvertToInternalValue(
                            Math.Max(0.5, roomType.EqualityToleranceMm)),
                        MergeChains = roomType.MergeChains,
                        MaxWitnessLine = maxWitnessLine,
                        TierStep = tierStep,
                        OverallMinSegments = roomType.OverallDimMinSegments,
                        DuplicateMaxDistance = UnitUtilsHelper.ConvertToInternalValue(
                            Math.Max(0, roomType.DuplicateMaxDistanceMm)),
                        PylonDuplicatesAcrossPylons = roomType.PylonDuplicatesAcrossPylons,
                        DoorLeaves = GetNearbyDoorObstacles(
                            contour, result.DoorObstacles.Leaves, l => l.Corners, textSize),
                        DoorOpeningArcs = GetNearbyDoorObstacles(
                            contour, result.DoorObstacles.OpeningArcs, a => a.Points, textSize)
                    };
                    roomResult.PlanningOptions = options;
                    roomResult.PlanningLog.Add($"Дверные полотна для текста: {options.DoorLeaves.Count}; "
                                              + $"дуг открывания: {options.DoorOpeningArcs.Count}");
                    roomResult.Dimensions = _planner.Plan(contour, options, roomResult.PlanningLog);
                    foreach(var dimension in roomResult.Dimensions) {
                        _dimensionBuilder.Create(dimension, dimensionType);
                    }
                }
            }

            transaction.Commit();
        }
        result.Warnings = warningSwallower.Messages;

        // Режим отладки: контур отдельной транзакцией (отменяется одним шагом), изображение вида и журнал
        if(settings.Debug.Enabled) {
            DrawContours(result);
            string folder = GetOutputFolder(settings, settingsPath);
            string baseName = $"roomdims_{DateTime.Now:yyyyMMdd_HHmmss}";
            ExportImage(folder, baseName, result);
            WriteDump(folder, baseName, settingsPath, result);
        }
        return result;
    }

    private static List<T> GetNearbyDoorObstacles<T>(
        RoomContour contour, List<T> obstacles, Func<T, IReadOnlyList<Point2D>> getPoints, double margin) {
        var points = contour.Loops.SelectMany(l => l.Lines)
            .SelectMany(l => new[] { l.Segment.Start, l.Segment.End }).ToList();
        if(points.Count == 0) {
            return [];
        }
        // Полотно может выходить из соседнего помещения в это: отбираем по перекрытию областей.
        double minX = points.Min(p => p.X) - margin;
        double maxX = points.Max(p => p.X) + margin;
        double minY = points.Min(p => p.Y) - margin;
        double maxY = points.Max(p => p.Y) + margin;
        return obstacles.Where(o => getPoints(o).Min(p => p.X) <= maxX && getPoints(o).Max(p => p.X) >= minX
                                    && getPoints(o).Min(p => p.Y) <= maxY && getPoints(o).Max(p => p.Y) >= minY)
            .ToList();
    }

    private void CollectViewDiagnostics(RoomDimensioningResult result) {
        try {
            // Снимок до создания новых размеров и цветной отладки, чтобы не записывать их как препятствия.
            result.ViewDiagnostics = new ViewObstacleCollector(
                _revitRepository.Document, _revitRepository.ActiveView, result.DoorObstacles.Geometry).Collect();
        } catch(Exception ex) {
            result.ViewDiagnosticsError = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private void DrawContours(RoomDimensioningResult result) {
        var warningSwallower = new WarningSwallower();
        using var transaction = new Transaction(
            _revitRepository.Document,
            _localizationService.GetLocalizedString("RoomDimensioningWindow.DebugContourTransaction"));
        var failureOptions = transaction.GetFailureHandlingOptions();
        failureOptions.SetFailuresPreprocessor(warningSwallower);
        transaction.SetFailureHandlingOptions(failureOptions);
        transaction.Start();

        result.DebugLineCount = _contourDrawer.Draw(
            result.Rooms.Where(r => r.Contour.Error == null).Select(r => r.Contour));

        transaction.Commit();
        result.Warnings.AddRange(warningSwallower.Messages);
    }

    private static string GetOutputFolder(RoomDimensioningSettings settings, string settingsPath) {
        // Без папки пишем рядом с файлом настроек
        return string.IsNullOrWhiteSpace(settings.Debug.OutputFolder)
            ? Path.GetDirectoryName(settingsPath)
            : settings.Debug.OutputFolder;
    }

    private void ExportImage(string folder, string baseName, RoomDimensioningResult result) {
        try {
            result.ImagePath = _imageExporter.Export(folder, baseName);
        } catch(Exception ex) {
            result.ImageError = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private void WriteDump(string folder, string baseName, string settingsPath, RoomDimensioningResult result) {
        try {
            result.DumpPath = _dumpWriter.Write(folder, baseName, settingsPath, result);
        } catch(Exception ex) {
            result.DumpError = $"{ex.GetType().Name}: {ex.Message}";
        }
    }
}
