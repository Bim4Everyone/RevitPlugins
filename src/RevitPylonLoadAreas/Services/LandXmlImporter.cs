using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

/// <summary>
/// Импортирует TIN поверхности из файлов LandXML
/// LandXML 1.2 spec: http://www.landxml.org/schema/LandXML-1.2/LandXML-1.2.xsd
/// </summary>
internal sealed class LandXmlImporter {
    private readonly RevitRepository _repo;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Точка съемки активного проекта. Должна совпадать с началом координат ГП
    /// </summary>
    private readonly BasePoint _surveyPoint;

    /// <summary>
    /// Координаты точки съемки в плоскости XOY относительно начала активного документа
    /// </summary>
    private readonly XYZ _surveyPointXYTranslation;

    public LandXmlImporter(RevitRepository repo, ILocalizationService localization) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        _surveyPoint = BasePoint.GetSurveyPoint(repo.Document);
        _surveyPointXYTranslation = new XYZ(_surveyPoint.Position.X, _surveyPoint.Position.Y, 0);
    }

    /// <summary>
    /// Загружает треугольники TIN поверхностей из файла LandXML.
    /// Координаты точек конвертируются во внутренние единицы Revit (футы),
    /// невидимые грани (i="1") пропускаются.
    /// </summary>
    /// <param name="path">Путь к файлу LandXML</param>
    /// <returns>Треугольники всех TIN поверхностей файла в координатах Revit</returns>
    public ICollection<Polygon3D> Import(string path) {
        if(!File.Exists(path)) {
            throw new FileNotFoundException(
                _localization.GetLocalizedString("Error.LandXmlFileNotFound"),
                path);
        }

        var doc = XDocument.Load(path);
        var root = doc.Root
                   ?? throw new InvalidOperationException(
                       _localization.GetLocalizedString("Error.LandXmlEmptyFile"));
        var ns = root.Name.Namespace;
        if(ns != XNamespace.Get("http://www.landxml.org/schema/LandXML-1.2")) {
            throw new InvalidOperationException(
                _localization.GetLocalizedString("Error.LandXmlVersionNotSupported", ns));
        }

        double feetPerUnit = GetFeetPerLinearUnit(root, ns);

        var polygons = new List<Polygon3D>();
        foreach(var definition in root.Elements(ns + "Surfaces")
                    .Elements(ns + "Surface")
                    .Elements(ns + "Definition")) {
            var points = GetPoints(definition, ns, feetPerUnit);
            polygons.AddRange(GetFaces(definition, ns, points));
        }

        return polygons;
    }

    private double GetFeetPerLinearUnit(XElement root, XNamespace ns) {
        string linearUnit = root.Element(ns + "Units")
            ?
            .Elements()
            .Select(e => e.Attribute("linearUnit")?.Value)
            .FirstOrDefault(v => v != null);
        return linearUnit switch {
            "millimeter" => UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Millimeters),
            "centimeter" => UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters),
            "meter" => UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Meters),
            "kilometer" => UnitUtils.ConvertToInternalUnits(1000, UnitTypeId.Meters),
            "inch" => UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Inches),
            "foot" => 1,
            "USSurveyFoot" => UnitUtils.ConvertToInternalUnits(1, UnitTypeId.UsSurveyFeet),
            "mile" => 5280,
            _ => throw new InvalidOperationException(
                _localization.GetLocalizedString("Error.LandXmlUnknownLinearUnit", linearUnit))
        };
    }

    private Dictionary<string, XYZ> GetPoints(XElement definition, XNamespace ns, double feetPerUnit) {
        var points = new Dictionary<string, XYZ>();
        foreach(var p in definition.Elements(ns + "Pnts").Elements(ns + "P")) {
            string id = p.Attribute("id")?.Value
                        ?? throw new InvalidOperationException(
                            _localization.GetLocalizedString("Error.LandXmlPointWithoutId"));
            double[] coords = ParseDoubles(p.Value);
            if(coords.Length < 3) {
                throw new InvalidOperationException(
                    _localization.GetLocalizedString("Error.LandXmlPointInvalidCoords", id));
            }

            // порядок координат по схеме LandXML: northing easting elevation
            var landXmlPoint = new XYZ(
                coords[1] * feetPerUnit,
                coords[0] * feetPerUnit,
                coords[2] * feetPerUnit);
            // преобразование системы координат из LandXML в систему координат ревита
            points[id] = landXmlPoint + _surveyPointXYTranslation;
        }

        return points;
    }

    private IEnumerable<Polygon3D> GetFaces(
        XElement definition,
        XNamespace ns,
        IReadOnlyDictionary<string, XYZ> points) {
        foreach(var f in definition.Elements(ns + "Faces").Elements(ns + "F")) {
            if(f.Attribute("i")?.Value == "1") {
                continue; // невидимая грань
            }

            var vertices = f.Value
                .Split((char[]) null, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => points.TryGetValue(id, out var xyz)
                    ? xyz
                    : throw new InvalidOperationException(
                        _localization.GetLocalizedString("Error.LandXmlFaceUnknownPoint", id)))
                .ToArray();
            if(IsDegeneratePolygon(vertices)) {
                continue; // вырожденный 3D полигон
            }

            if(IsDegeneratePolygon(vertices.Select(v => new XY(v)).ToArray())) {
                continue; // проекция полигона на XOY - вырожденный 2D полигон 
            }
            yield return new Polygon3D(vertices);
        }
    }

    private bool IsDegeneratePolygon(XYZ[] points) {
        if(points.Length < 3) {
            return true;
        }

        double tolerance = _repo.Application.ShortCurveTolerance;
        if(points[0].DistanceTo(points[points.Length - 1]) <= tolerance) {
            return true;
        }

        for(int i = 0; i < points.Length - 1; i++) {
            if(points[i].DistanceTo(points[i + 1]) <= tolerance) {
                return true;
            }
        }

        return false;
    }

    private bool IsDegeneratePolygon(XY[] points) {
        if(points.Length < 3) {
            return true;
        }

        double tolerance = _repo.Application.ShortCurveTolerance;
        if(points[0].DistanceTo(points[points.Length - 1]) <= tolerance) {
            return true;
        }

        for(int i = 0; i < points.Length - 1; i++) {
            if(points[i].DistanceTo(points[i + 1]) <= tolerance) {
                return true;
            }
        }

        return false;
    }

    private double[] ParseDoubles(string value) {
        return value
            .Split((char[]) null, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
    }
}
