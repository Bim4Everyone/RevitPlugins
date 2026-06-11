using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

/// <summary>
/// Импортирует TIN поверхности из файлов LandXML
/// LandXML 1.2 spec: http://www.landxml.org/schema/LandXML-1.2/LandXML-1.2.xsd
/// </summary>
internal sealed class LandXmlImporter {
    private readonly ILocalizationService _localizationService;

    public LandXmlImporter(ILocalizationService localizationService) {
        _localizationService = localizationService
                               ?? throw new ArgumentNullException(nameof(localizationService));
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
                _localizationService.GetLocalizedString("Error.LandXmlFileNotFound"),
                path);
        }

        var doc = XDocument.Load(path);
        var root = doc.Root
                   ?? throw new InvalidOperationException(
                       _localizationService.GetLocalizedString("Error.LandXmlEmptyFile"));
        var ns = root.Name.Namespace;
        if(ns != XNamespace.Get("http://www.landxml.org/schema/LandXML-1.2")) {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("Error.LandXmlVersionNotSupported", ns));
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
                _localizationService.GetLocalizedString("Error.LandXmlUnknownLinearUnit", linearUnit))
        };
    }

    private Dictionary<string, XYZ> GetPoints(XElement definition, XNamespace ns, double feetPerUnit) {
        var points = new Dictionary<string, XYZ>();
        foreach(var p in definition.Elements(ns + "Pnts").Elements(ns + "P")) {
            string id = p.Attribute("id")?.Value
                        ?? throw new InvalidOperationException(
                            _localizationService.GetLocalizedString("Error.LandXmlPointWithoutId"));
            double[] coords = ParseDoubles(p.Value);
            if(coords.Length < 3) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Error.LandXmlPointInvalidCoords", id));
            }

            // порядок координат по схеме LandXML: northing easting elevation
            points[id] = new XYZ(
                coords[1] * feetPerUnit,
                coords[0] * feetPerUnit,
                coords[2] * feetPerUnit);
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
                        _localizationService.GetLocalizedString("Error.LandXmlFaceUnknownPoint", id)))
                .ToArray();
            yield return new Polygon3D(vertices);
        }
    }

    private double[] ParseDoubles(string value) {
        return value
            .Split((char[]) null, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
    }
}
