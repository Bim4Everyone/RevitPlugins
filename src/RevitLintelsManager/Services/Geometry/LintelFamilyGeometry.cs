using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Читает признаки поддерживаемой схемы семейства из загруженного определения.</summary>
internal static class LintelFamilyGeometry {
    /// <summary>Подтверждает ось и знак левой стороны по управляемым размерам и связям креплений.</summary>
    public static int? ReadLeftSign(Document definition, IReadOnlyDictionary<string, string> mappings,
        FamilyOriginFacts planes) {
        if(planes.Full is not { } origin) {
            return null;
        }
        using var dimensions = new FilteredElementCollector(definition).OfClass(typeof(Dimension));
        var all = dimensions.Cast<Dimension>().ToArray();
        var labeled = all
            .Select(dimension => (Dimension: dimension, Label: ReadOptional(() => dimension.FamilyLabel)))
            .Where(item => item.Label is not null).ToLookup(item => item.Label.Definition.Name, item => item.Dimension);
        var width = ReadPlaneOffsets(definition, labeled[Name(mappings, nameof(LintelFamilyConfig.LintelWidth))],
            XYZ.BasisX);
        var thickness = ReadPlaneOffsets(definition,
            labeled[Name(mappings, nameof(LintelFamilyConfig.LintelThickness))], XYZ.BasisY);
        if(width is not { } x || thickness is not { } y
            || !HasCenterConstraint(definition, all, x, origin.X)
            || !HasCenterConstraint(definition, all, y, origin.Y)) {
            return null;
        }
        int left = ReadSide(definition, labeled[Name(mappings, nameof(LintelFamilyConfig.LintelLeftOffset))], x);
        int right = ReadSide(definition, labeled[Name(mappings, nameof(LintelFamilyConfig.LintelRightOffset))], x);
        if(left == 0 || left != -right) {
            return null;
        }
        using var components = new FilteredElementCollector(definition).OfClass(typeof(FamilyInstance));
        var visibility = components.Cast<FamilyInstance>().Select(instance => {
            using var transform = instance.GetTransform();
            var parameter = instance.get_Parameter(BuiltInParameter.IS_VISIBLE_PARAM);
            var control = parameter is null ? null : definition.FamilyManager.GetAssociatedFamilyParameter(parameter);
            return (Origin: AsPoint(transform.Origin), Control: control);
        }).Where(component => component.Control is not null).ToArray();
        return HasAttachments(visibility, left, Name(mappings, nameof(LintelFamilyConfig.LintelLeftCorner)),
                   Name(mappings, nameof(LintelFamilyConfig.LintelLeftWelding)))
            && HasAttachments(visibility, right, Name(mappings, nameof(LintelFamilyConfig.LintelRightCorner)),
                   Name(mappings, nameof(LintelFamilyConfig.LintelRightWelding))) ? left : null;
    }

    // Возвращает настройку по роли, не используя имя конкретного семейства или параметра.
    private static string Name(IReadOnlyDictionary<string, string> mappings, string key) {
        return mappings.TryGetValue(key, out string name) ? name : null;
    }

    // Проверяет линейный размер между двумя опорными плоскостями вдоль заданной локальной оси.
    private static (ElementId Start, ElementId End, double Min, double Max)? ReadPlaneOffsets(Document definition,
        IEnumerable<Dimension> matches, XYZ axis) {
        var dimensions = matches.Take(2).ToArray();
        if(dimensions.Length != 1 || dimensions[0].References is not { Size: 2 } references) {
            return null;
        }
        var values = new List<(ElementId Id, double Offset)>(2);
        foreach(Reference reference in references) {
            if(definition.GetElement(reference.ElementId) is not ReferencePlane datum) {
                return null;
            }
            using var plane = datum.GetPlane();
            if(plane.Normal.CrossProduct(axis).GetLength() > DirectionEpsilon) {
                return null;
            }
            values.Add((datum.Id, plane.Origin.DotProduct(axis)));
        }
        var start = values[0].Offset <= values[1].Offset ? values[0] : values[1];
        var end = start.Id == values[0].Id ? values[1] : values[0];
        return (start.Id, end.Id, start.Offset, end.Offset);
    }

    // Проверяет, что размеры сохраняют симметрию при изменении ширины или толщины.
    private static bool HasCenterConstraint(Document definition, IEnumerable<Dimension> dimensions,
        (ElementId Start, ElementId End, double Min, double Max) span, string originName) {
        return span.Max > LengthEpsilon && Math.Abs(span.Min + span.Max) <= LengthEpsilon
            && dimensions.Any(dimension => ReadOptional(() => IsEqualDimension(definition, dimension,
                span.Start, span.End, originName)));
    }

    // Связывает ограничение EQ с теми же граничными плоскостями и центральной плоскостью начала.
    private static bool IsEqualDimension(Document definition, Dimension dimension, ElementId start, ElementId end,
        string originName) {
        if(dimension.NumberOfSegments != 2 || !dimension.AreSegmentsEqual
            || dimension.References is not { Size: 3 } references) {
            return false;
        }
        var a = references.get_Item(0).ElementId;
        var b = references.get_Item(2).ElementId;
        return ((a == start && b == end) || (a == end && b == start))
            && definition.GetElement(references.get_Item(1).ElementId) is ReferencePlane origin
            && origin.Name == originName
            && origin.GetParamValue<int>(BuiltInParameter.DATUM_PLANE_DEFINES_ORIGIN) == 1;
    }

    // Размер опирания начинается на соответствующей границе проёма и продолжается наружу.
    private static int ReadSide(Document definition, IEnumerable<Dimension> dimensions,
        (ElementId Start, ElementId End, double Min, double Max) width) {
        if(ReadPlaneOffsets(definition, dimensions, XYZ.BasisX) is not { } span) {
            return 0;
        }
        if(span.End == width.Start && span.Min < width.Min - LengthEpsilon) {
            return -1;
        }
        return span.Start == width.End && span.Max > width.Max + LengthEpsilon ? 1 : 0;
    }

    // Проверяет управляющие связи видимости уголка и анкеров на той же физической стороне семейства.
    private static bool HasAttachments(IReadOnlyCollection<(GeometryPoint Origin, FamilyParameter Control)> items,
        int sign, string corner, string welding) {
        if(string.IsNullOrWhiteSpace(corner) || string.IsNullOrWhiteSpace(welding)) {
            return false;
        }
        var corners = items.Where(item => item.Control.Definition.Name == corner).ToArray();
        if(corners.Length == 0 || corners.Any(item => item.Origin.X * sign <= LengthEpsilon)
            || !corners.Any(item => Math.Abs(item.Origin.Z) <= LengthEpsilon)) {
            return false;
        }
        // У дополнительных анкеров допускается ещё один флаг включения в начале and().
        string pattern = @"^\s*and\s*\(\s*(?:[^(),]+,\s*)?" + Regex.Escape(corner)
            + @"\s*,\s*not\s*\(\s*" + Regex.Escape(welding) + @"\s*\)\s*\)\s*$";
        var anchors = items.Where(item => item.Control.Formula is string formula
            && Regex.IsMatch(formula, pattern, RegexOptions.CultureInvariant)).ToArray();
        return anchors.Length > 0 && anchors.All(item => item.Origin.X * sign > LengthEpsilon);
    }
}
