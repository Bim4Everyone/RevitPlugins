using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Debug;
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
        double maxShift = UnitUtilsHelper.ConvertToInternalValue(settings.Layout.MaxShiftPrintMm * scale);
        double maxLeader = UnitUtilsHelper.ConvertToInternalValue(settings.Layout.MaxLeaderPrintMm * scale);
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
                    var options = new PlanningOptions {
                        Offset = offset,
                        TextAllowance = DimensionTextHelper.GetTextAllowance(dimensionType, scale),
                        // Плоскости и значения сравниваются с допуском типа помещения, но не точнее 0.5 мм
                        CoordinateTolerance = UnitUtilsHelper.ConvertToInternalValue(
                            Math.Max(0.5, roomType.EqualityToleranceMm)),
                        MaxShift = maxShift,
                        MaxLeader = maxLeader,
                        TierStep = tierStep,
                        OverallMinSegments = roomType.OverallDimMinSegments
                    };
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
