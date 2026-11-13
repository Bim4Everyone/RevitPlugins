using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Diagnostics;

/// <summary>
/// Пишет подробный журнал запуска в JSON для разбора результатов.
/// </summary>
internal class DebugDumpWriter {
    private readonly RevitRepository _revitRepository;

    public DebugDumpWriter(RevitRepository revitRepository) {
        _revitRepository = revitRepository.ThrowIfNull();
    }

    /// <returns>Путь к записанному файлу.</returns>
    public string Write(string outputFolder, string baseName, string settingsPath, RoomDimensioningResult result) {
        Directory.CreateDirectory(outputFolder);
        string path = Path.Combine(outputFolder, baseName + ".json");

        var view = _revitRepository.ActiveView;
        var dump = new {
            RunAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            RevitVersion = _revitRepository.Application.VersionNumber,
            Document = _revitRepository.Document.Title,
            View = view.Name,
            ViewScale = view.Scale,
            SettingsPath = settingsPath,
            result.ImagePath,
            result.ImageError,
            Summary = new {
                Rooms = result.Rooms.Count,
                result.UnmatchedRoomCount,
                result.RoomErrorCount,
                Lines = result.LineCount,
                LinesWithoutReference = result.LinesWithoutReferenceCount,
                Dimensions = result.DimensionCount,
                DimensionsOk = result.DimensionOkCount,
                DimensionsByStatus = result.Rooms
                    .SelectMany(r => r.Dimensions)
                    .GroupBy(d => d.Status ?? "NotCreated")
                    .ToDictionary(g => g.Key, g => g.Count()),
                TextIssues = result.Rooms
                    .SelectMany(r => r.Dimensions)
                    .SelectMany(d => d.TextIssues)
                    .GroupBy(k => k)
                    .ToDictionary(g => g.Key, g => g.Count()),
                DeletedDimensions = result.DeletedDimensionCount
            },
            RevitWarnings = result.Warnings,
            result.ViewDiagnostics,
            result.ViewDiagnosticsError,
            DoorObstacles = new {
                result.DoorObstacles.DoorCount,
                Leaves = result.DoorObstacles.Leaves.Select(l => new {
                    l.DoorId,
                    CornersMm = l.Corners.Select(Mm).ToList()
                }).ToList(),
                OpeningArcs = result.DoorObstacles.OpeningArcs.Select(a => new {
                    a.DoorId,
                    PointsMm = a.Points.Select(Mm).ToList()
                }).ToList(),
                result.DoorObstacles.WithoutRecognizedLeaf,
                result.DoorObstacles.Errors
            },
            Rooms = result.Rooms.Select(DumpRoom).ToList()
        };

        string json = JsonConvert.SerializeObject(dump, Formatting.Indented);
        File.WriteAllText(path, json, new UTF8Encoding(false));
        return path;
    }

    private object DumpRoom(RoomRunResult roomResult) {
        var contour = roomResult.Contour;
        return new {
            RoomId = IdValue(contour.Room.Id),
            contour.Room.Number,
            Name = contour.Room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString(),
            RoomType = contour.RoomType.Name,
            contour.BoundaryLocation,
            contour.Error,
            TextMetrics = roomResult.PlanningOptions is null ? null : new {
                SizeMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(roomResult.PlanningOptions.TextSize), 3),
                GapMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(roomResult.PlanningOptions.TextGap), 3),
                WidthScale = roomResult.PlanningOptions.TextWidthScale,
                DoorLeaves = roomResult.PlanningOptions.DoorLeaves.Count,
                DoorOpeningArcs = roomResult.PlanningOptions.DoorOpeningArcs.Count,
                DoorTextClearanceMm = Math.Round(
                    UnitUtilsHelper.ConvertFromInternalValue(roomResult.PlanningOptions.DoorTextClearance), 3)
            },
            Loops = contour.Loops.Select(loop => new {
                loop.Index,
                loop.IsOuter,
                AreaM2 = Math.Round(Math.Abs(loop.SignedArea) * 0.09290304, 3),
                Orientation = loop.SignedArea > 0 ? "CCW" : "CW",
                loop.RawLineCount,
                loop.NormalizationLog,
                Elements = loop.Elements.Select(DumpElement).ToList()
            }).ToList(),
            roomResult.PlanningLog,
            Dimensions = roomResult.Dimensions.Select(d => new {
                d.Id,
                d.Kind,
                d.InvolvesInner,
                DirectionDeg = Math.Round(Math.Atan2(d.Direction.Y, d.Direction.X) * 180 / Math.PI, 3),
                SideSign = d.SideSign,
                LinePositionMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(d.LinePosition), 1),
                ContourPositionMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(d.ContourPosition), 1),
                MaxWitnessLineMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(d.MaxWitnessLength), 1),
                d.Sources,
                d.TextIssues,
                Anchors = d.Anchors.Select(a => new {
                    CoordinateMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(a.Coordinate), 1),
                    Element = $"{a.LoopIndex}:{a.ElementIndex}"
                }).ToList(),
                ExpectedSegmentsMm = d.ExpectedSegments
                    .Select(v => Math.Round(UnitUtilsHelper.ConvertFromInternalValue(v), 1))
                    .ToList(),
                ActualSegmentsMm = d.ActualSegmentsMm
                    .Select(v => v.HasValue ? Math.Round(v.Value, 1) : (double?) null)
                    .ToList(),
                DimensionId = d.DimensionId is null ? (long?) null : IdValue(d.DimensionId),
                d.Status,
                d.Error
            }).ToList()
        };
    }

    private object DumpElement(ContourElement element) {
        return element switch {
            ContourLine line => (object) new {
                Kind = line.IsArc ? "Arc" : "Line",
                line.Index,
                line.RawIndices,
                line.IsFiltered,
                StartMm = Mm(line.Segment.Start),
                EndMm = Mm(line.Segment.End),
                LengthMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(line.Segment.Length), 1),
                DirectionDeg = Math.Round(Math.Atan2(line.Segment.Direction.Y, line.Segment.Direction.X) * 180 / Math.PI, 3),
                InwardNormal = new[] { Math.Round(line.InwardNormal.X, 4), Math.Round(line.InwardNormal.Y, 4) },
                Source = DumpSource(line),
                InferredSourceId = line.InferredSourceId is null ? (long?) null : IdValue(line.InferredSourceId),
                Reference = DumpReference(line)
            },
            ContourPoint point => (object) new {
                Kind = "ContourPoint",
                point.Index,
                AtMm = Mm(point.Location),
                AngleDeg = Math.Round(point.AngleDeg, 3),
                Reference = DumpReference(point)
            },
            _ => null
        };
    }

    private object DumpSource(ContourLine line) {
        if(line.SourceId is null || line.SourceId == ElementId.InvalidElementId) {
            return null;
        }
        var element = _revitRepository.Document.GetElement(line.SourceId);
        bool isLink = line.LinkSourceId != null && line.LinkSourceId != ElementId.InvalidElementId;
        return new {
            Id = IdValue(line.SourceId),
            LinkElementId = isLink ? IdValue(line.LinkSourceId) : (long?) null,
            Class = element?.GetType().Name,
            Category = element?.Category?.Name,
            TypeName = element is null
                ? null
                : _revitRepository.Document.GetElement(element.GetTypeId())?.Name
        };
    }

    private object DumpReference(ContourElement element) {
        string stable = null;
        if(element.Reference != null) {
            try {
                stable = element.Reference.ConvertToStableRepresentation(_revitRepository.Document);
            } catch(Exception ex) {
                stable = $"<{ex.GetType().Name}>";
            }
        }
        return new {
            Found = element.Reference != null,
            Method = element.ReferenceMethod,
            Stable = stable,
            Error = element.ReferenceError
        };
    }

    private static double[] Mm(Point2D point) {
        return [
            Math.Round(UnitUtilsHelper.ConvertFromInternalValue(point.X), 1),
            Math.Round(UnitUtilsHelper.ConvertFromInternalValue(point.Y), 1)
        ];
    }

    private static long IdValue(ElementId id) {
#if REVIT_2024_OR_GREATER
        return id.Value;
#else
        return id.IntegerValue;
#endif
    }
}
