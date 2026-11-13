using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Diagnostics;

/// <summary>
/// Диагностический снимок потенциальных препятствий до создания размеров и отладочных линий (D76).
/// Ничего не скрывает и не изменяет; габариты и доступная геометрия не являются маской видимых пикселей.
/// Двери имеют приоритет, сантехника и специальное оборудование собираются для следующего этапа.
/// </summary>
internal class ViewObstacleCollector {
    private const int _elementLimit = 1000;
    private readonly Document _document;
    private readonly View _view;
    private readonly IReadOnlyDictionary<long, DoorGeometryData> _doorGeometry;

    public ViewObstacleCollector(
        Document document, View view, IReadOnlyDictionary<long, DoorGeometryData> doorGeometry = null) {
        _document = document.ThrowIfNull();
        _view = view.ThrowIfNull();
        _doorGeometry = doorGeometry;
    }

    public object Collect() {
        var errors = new List<string>();
        using var collector = new FilteredElementCollector(_document, _view.Id);
        var candidates = collector.WhereElementIsNotElementType()
            .Where(e => e.Category != null && !e.IsHidden(_view))
            .ToList();
        var selected = candidates.OrderBy(Priority).ThenBy(e => IdValue(e.Id)).Take(_elementLimit).ToList();
        var elements = new List<object>();
        foreach(var element in selected) {
            try {
                elements.Add(ReadElement(element));
            } catch(Exception ex) {
                errors.Add($"Element {IdValue(element.Id)}: {ex.GetType().Name}: {ex.Message}");
            }
        }
        return new {
            SchemaVersion = 1,
            Stage = "AfterPluginDimensionDeletionBeforeCreation",
            Scope = "Host document, view collector; may include occluded or outside-crop geometry",
            LinkedContentsCollected = false,
            Candidates = candidates.Count,
            Selected = selected.Count,
            Collected = elements.Count,
            ElementLimit = _elementLimit,
            Truncated = candidates.Count > selected.Count,
            Units = "mm",
            ViewFrame = new {
                OriginMm = Mm(_view.Origin),
                Right = Vector(_view.RightDirection),
                Up = Vector(_view.UpDirection),
                Normal = Vector(_view.ViewDirection)
            },
            Categories = candidates.GroupBy(e => IdValue(e.Category.Id))
                .OrderBy(g => g.Key)
                .Select(g => new {
                    Id = g.Key,
                    Name = g.First().Category.Name,
                    Kind = g.First().Category.CategoryType.ToString(),
                    Role = Role(g.First()),
                    Count = g.Count(),
                    Selected = selected.Count(e => IdValue(e.Category.Id) == g.Key)
                }).ToList(),
            Elements = elements,
            Errors = errors
        };
    }

    private object ReadElement(Element element) {
        var errors = new List<string>();
        var instance = element as FamilyInstance;
        var type = _document.GetElement(element.GetTypeId());
        object box = TryRead(() => ReadBox(element.get_BoundingBox(_view)), "Bounds", errors);
        object location = TryRead(() => ReadLocation(element), "Location", errors);
        object door = null;
        object geometry = null;
        if(IsCategory(element, BuiltInCategory.OST_Doors)) {
            door = TryRead(() => ReadDoor(instance), "Door", errors);
            geometry = TryRead(() => ReadDoorGeometry(element), "Geometry", errors);
        }
        object link = null;
        if(element is RevitLinkInstance linkInstance) {
            link = TryRead(() => new {
                ContentsCollected = false,
                Loaded = linkInstance.GetLinkDocument() != null,
                Transform = DumpTransform(linkInstance.GetTotalTransform())
            }, "Link", errors);
        }
        return new {
            Id = IdValue(element.Id),
            CategoryId = IdValue(element.Category.Id),
            Category = element.Category.Name,
            Class = element.GetType().Name,
            Role = Role(element),
            Family = instance?.Symbol?.FamilyName,
            TypeName = type?.Name,
            TypeId = IdValue(element.GetTypeId()),
            OwnerViewId = IdValue(element.OwnerViewId),
            Bounds = box,
            Location = location,
            Door = door,
            ViewGeometry = geometry,
            Link = link,
            Errors = errors
        };
    }

    private object ReadDoorGeometry(Element element) {
        if(_doorGeometry != null && _doorGeometry.TryGetValue(IdValue(element.Id), out var geometry)) {
            return geometry;
        }
        return new DoorGeometryCollector(_document, _view).Collect(element);
    }

    private object ReadBox(BoundingBoxXYZ box) {
        if(box is null) {
            return null;
        }
        var corners = new List<XYZ>();
        foreach(double x in new[] { box.Min.X, box.Max.X }) {
            foreach(double y in new[] { box.Min.Y, box.Max.Y }) {
                foreach(double z in new[] { box.Min.Z, box.Max.Z }) {
                    corners.Add(box.Transform.OfPoint(new XYZ(x, y, z)));
                }
            }
        }
        var projected = corners.Select(Project).ToList();
        return new {
            ModelMinMm = Mm(new XYZ(corners.Min(p => p.X), corners.Min(p => p.Y), corners.Min(p => p.Z))),
            ModelMaxMm = Mm(new XYZ(corners.Max(p => p.X), corners.Max(p => p.Y), corners.Max(p => p.Z))),
            ViewMinMm = new[] { projected.Min(p => p[0]), projected.Min(p => p[1]) },
            ViewMaxMm = new[] { projected.Max(p => p[0]), projected.Max(p => p[1]) },
            Note = "Coarse API bounds; not a door-leaf or visible-ink mask"
        };
    }

    private object ReadLocation(Element element) {
        if(element.Location is LocationPoint point) {
            return new { Kind = "Point", ModelMm = Mm(point.Point), ViewMm = Project(point.Point), point.Rotation };
        }
        Curve curve = element.Location is LocationCurve line ? line.Curve
            : element is Grid grid ? grid.Curve
            : element is CurveElement curveElement ? curveElement.GeometryCurve
            : null;
        if(curve is null || !curve.IsBound) {
            return null;
        }
        return new {
            Kind = curve.GetType().Name,
            StartMm = Mm(curve.GetEndPoint(0)),
            EndMm = Mm(curve.GetEndPoint(1)),
            StartViewMm = Project(curve.GetEndPoint(0)),
            EndViewMm = Project(curve.GetEndPoint(1))
        };
    }

    private static object ReadDoor(FamilyInstance instance) {
        return instance is null ? null : new {
            HostId = instance.Host is null ? (long?) null : IdValue(instance.Host.Id),
            Facing = Vector(instance.FacingOrientation),
            Hand = Vector(instance.HandOrientation),
            instance.FacingFlipped,
            instance.HandFlipped,
            instance.Mirrored,
            Note = "Orientation only; open-leaf geometry must be checked against PNG"
        };
    }

    private static object DumpTransform(Transform transform) {
        return new {
            OriginMm = Mm(transform.Origin),
            X = Vector(transform.BasisX),
            Y = Vector(transform.BasisY),
            Z = Vector(transform.BasisZ)
        };
    }

    private double[] Project(XYZ point) {
        var relative = point - _view.Origin;
        return new[] { ToMm(relative.DotProduct(_view.RightDirection)), ToMm(relative.DotProduct(_view.UpDirection)) };
    }

    private static object TryRead(Func<object> read, string part, List<string> errors) {
        try {
            return read();
        } catch(Exception ex) {
            errors.Add($"{part}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static int Priority(Element element) {
        return IsCategory(element, BuiltInCategory.OST_Doors) ? 0
            : IsCategory(element, BuiltInCategory.OST_PlumbingFixtures)
              || IsCategory(element, BuiltInCategory.OST_SpecialityEquipment) ? 1 : 2;
    }

    private static string Role(Element element) {
        if(IsCategory(element, BuiltInCategory.OST_Doors)) {
            return "DoorPriority";
        }
        if(IsCategory(element, BuiltInCategory.OST_PlumbingFixtures)
           || IsCategory(element, BuiltInCategory.OST_SpecialityEquipment)) {
            return "EquipmentLater";
        }
        return element is RevitLinkInstance ? "LinkContentsNotCollected" : "InventoryOnly";
    }

    private static bool IsCategory(Element element, BuiltInCategory category) {
        return element.Category != null && IdValue(element.Category.Id) == (long) category;
    }

    private static double ToMm(double value) {
        return Math.Round(UnitUtilsHelper.ConvertFromInternalValue(value), 2);
    }

    private static double[] Mm(XYZ point) {
        return new[] { ToMm(point.X), ToMm(point.Y), ToMm(point.Z) };
    }

    private static double[] Vector(XYZ vector) {
        return new[] { Math.Round(vector.X, 6), Math.Round(vector.Y, 6), Math.Round(vector.Z, 6) };
    }

    private static long IdValue(ElementId id) {
#if REVIT_2024_OR_GREATER
        return id.Value;
#else
        return id.IntegerValue;
#endif
    }
}
