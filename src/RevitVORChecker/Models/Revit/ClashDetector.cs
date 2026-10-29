using System.Diagnostics;

using Autodesk.Revit.DB;

using RevitVORChecker.Models.Data;

namespace RevitVORChecker.Models.Revit;

/// <summary>
/// Поиск пересечений тел конструкций глубже допуска и полных дублей средствами Revit API. Модель не изменяется.
/// </summary>
/// <remarks>
/// Аналог проверок Navisworks «КР-КР». Изоляция (типы с «Изол») не проверяется — она по технологии примыкает к бетону.
/// </remarks>
internal static class ClashDetector {
    private const double FeetToMillimeters = 304.8;
    private const double CubicFeetToCubicMeters = 0.028316846592;
    private const double MinSolidVolume = 1e-6;

    private static readonly BuiltInCategory[] _categories = {
        BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs, BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Stairs
    };

    /// <summary>
    /// Конструкции группы «Прочее» (остальные — «ЖБ»).
    /// </summary>
    private static readonly string[] _otherGroupKeys = { "БПодготовка", "ПСлой", "Засыпка" };

    public static ClashesData Detect(Document document, double toleranceMm) {
        var stopwatch = Stopwatch.StartNew();
        double tolerance = toleranceMm / FeetToMillimeters;
        WorksetTable worksets = document.IsWorkshared ? document.GetWorksetTable() : null;
        string GetWorkset(Element element) {
            return worksets?.GetWorkset(element.WorksetId)?.Name ?? "";
        }

        string GetTypeName(Element element) {
            Element type = document.GetElement(element.GetTypeId());
            return type == null ? element.Name : type.Name;
        }

        var elements = new FilteredElementCollector(document)
            .WherePasses(new ElementMulticategoryFilter(_categories))
            .WhereElementIsNotElementType()
            .Where(element => GetTypeName(element).IndexOf("Изол", StringComparison.OrdinalIgnoreCase) < 0
                              && GetWorkset(element).IndexOf("спомогат", StringComparison.OrdinalIgnoreCase) < 0
                              && !(element is FamilyInstance { SuperComponent: not null })
                              && element.get_BoundingBox(null) != null)
            .ToList();
        var elementIds = elements.Select(element => element.Id).ToList();
        var solidsCache = new Dictionary<long, List<Solid>>();
        var result = new ClashesData {
            FileTitle = document.Title,
            ToleranceMm = toleranceMm,
            ElementCount = elements.Count
        };
        foreach(Element first in elements) {
            BoundingBoxXYZ firstBox = first.get_BoundingBox(null);
            var nearElements = new FilteredElementCollector(document, elementIds)
                .WherePasses(new BoundingBoxIntersectsFilter(new Outline(firstBox.Min, firstBox.Max), 0))
                .ToElements();
            foreach(Element second in nearElements) {
                long firstId = RevitIds.GetValue(first.Id);
                long secondId = RevitIds.GetValue(second.Id);
                // каждая пара проверяется один раз — от элемента с меньшим Id
                if(secondId <= firstId) {
                    continue;
                }

                BoundingBoxXYZ secondBox = second.get_BoundingBox(null);
                if(IsDuplicate(first, second, firstBox, secondBox)) {
                    result.Duplicates.Add(new DuplicateData {
                        FirstId = firstId,
                        SecondId = secondId,
                        Category = first.Category.Name,
                        TypeName = GetTypeName(first).Replace("\"", "'")
                    });
                    continue;
                }

                result.CheckedPairCount++;
                (double volume, double depth) = MeasureIntersection(
                    GetSolids(first, solidsCache),
                    GetSolids(second, solidsCache),
                    result);
                if(volume > 0 && depth >= tolerance) {
                    result.Clashes.Add(new ClashData {
                        FirstId = firstId,
                        SecondId = secondId,
                        FirstGroup = GetGroup(GetTypeName(first)),
                        SecondGroup = GetGroup(GetTypeName(second)),
                        FirstTitle = (first.Category.Name + ": " + GetTypeName(first)).Replace("\"", "'"),
                        SecondTitle = (second.Category.Name + ": " + GetTypeName(second)).Replace("\"", "'"),
                        VolumeM3 = Math.Round(volume * CubicFeetToCubicMeters, 4, MidpointRounding.AwayFromZero),
                        DepthMm = Math.Round(depth * FeetToMillimeters, MidpointRounding.AwayFromZero),
                        IsJoined = AreJoined(document, first, second)
                    });
                }
            }
        }

        result.Seconds = (int) stopwatch.Elapsed.TotalSeconds;
        return result;
    }

    /// <summary>
    /// Дубль: та же категория и тип, габариты совпадают с точностью 1 мм.
    /// </summary>
    private static bool IsDuplicate(Element first, Element second, BoundingBoxXYZ firstBox, BoundingBoxXYZ secondBox) {
        double precision = 1 / FeetToMillimeters;
        return first.Category.Id == second.Category.Id
               && first.GetTypeId() == second.GetTypeId()
               && firstBox.Min.DistanceTo(secondBox.Min) < precision
               && firstBox.Max.DistanceTo(secondBox.Max) < precision;
    }

    /// <summary>
    /// Наибольший объём пересечения тел двух элементов и глубина этого пересечения (наименьший размер его габарита).
    /// </summary>
    private static (double Volume, double Depth) MeasureIntersection(
        List<Solid> firstSolids,
        List<Solid> secondSolids,
        ClashesData result) {

        double maxVolume = 0;
        double depth = 0;
        foreach(Solid firstSolid in firstSolids) {
            foreach(Solid secondSolid in secondSolids) {
                Solid intersection;
                try {
                    intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                        firstSolid,
                        secondSolid,
                        BooleanOperationsType.Intersect);
                } catch(Exception) {
                    result.GeometryErrorCount++;
                    continue;
                }

                if(intersection == null || intersection.Volume < MinSolidVolume) {
                    continue;
                }

                BoundingBoxXYZ box = intersection.GetBoundingBox();
                XYZ size = box.Max - box.Min;
                if(intersection.Volume > maxVolume) {
                    maxVolume = intersection.Volume;
                    depth = Math.Min(size.X, Math.Min(size.Y, size.Z));
                }
            }
        }

        return (maxVolume, depth);
    }

    private static List<Solid> GetSolids(Element element, Dictionary<long, List<Solid>> cache) {
        long id = RevitIds.GetValue(element.Id);
        if(cache.TryGetValue(id, out List<Solid> solids)) {
            return solids;
        }

        solids = new List<Solid>();
        var options = new Options {
            ComputeReferences = false,
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = false
        };
        var pending = new Stack<GeometryElement>();
        GeometryElement geometry = element.get_Geometry(options);
        if(geometry != null) {
            pending.Push(geometry);
        }

        while(pending.Count > 0) {
            foreach(GeometryObject geometryObject in pending.Pop()) {
                if(geometryObject is Solid solid && solid.Volume > MinSolidVolume) {
                    solids.Add(solid);
                } else if(geometryObject is GeometryInstance instance) {
                    GeometryElement instanceGeometry = instance.GetInstanceGeometry();
                    if(instanceGeometry != null) {
                        pending.Push(instanceGeometry);
                    }
                }
            }
        }

        cache[id] = solids;
        return solids;
    }

    private static string GetGroup(string typeName) {
        return _otherGroupKeys.Any(typeName.Contains) ? "Прочее" : "ЖБ";
    }

    private static bool AreJoined(Document document, Element first, Element second) {
        try {
            return JoinGeometryUtils.AreElementsJoined(document, first, second);
        } catch(Exception) {
            return false;
        }
    }
}
