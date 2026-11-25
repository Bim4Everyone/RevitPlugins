using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;

internal class StructColumnPylon : IPylon {
    // Перевод из системы координат документа пилона в систему координат текущего проекта
    private readonly Transform _transform;

    /// <summary>
    /// Создает пилон из несущей колонны текущего проекта.
    /// </summary>
    public StructColumnPylon(Element element)
        : this(element, Transform.Identity) {
    }

    /// <summary>
    /// Создает пилон из несущей колонны текущего проекта или связанного файла.
    /// </summary>
    /// <param name="element">Несущая колонна.</param>
    /// <param name="transform">
    /// Перевод координат документа колонны в координаты текущего проекта.
    /// Для связи — <see cref="RevitLinkInstance.GetTotalTransform"/>.
    /// </param>
    public StructColumnPylon(Element element, Transform transform) {
        _transform = transform;

        var instance = (FamilyInstance) element;
        PylonInstance = instance;
        FacingOrientation = _transform.OfVector(instance.FacingOrientation);
        (Length, Width) = GetSectionDimensions(instance.Symbol);
    }

    public Element PylonInstance { get; }

    /// <summary>
    /// Нормаль к одной из коротких сторон пилона в координатах текущего проекта.
    /// </summary>
    public XYZ FacingOrientation { get; }

    public double Length { get; }
    public double Width { get; }

    public static (double Length, double Width) GetSectionDimensions(FamilySymbol symbol) {
        if(symbol is null) {
            return default;
        }

        double length = symbol.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeLength, 0);
        double width = symbol.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeWidth, 0);
        bool hasSharedDimensions = length > 0 && width > 0;
        if(hasSharedDimensions && (ContainsNamePart(symbol, "пилон") || ContainsNamePart(symbol, "pylon"))) {
            return (length, width);
        }

        using StructuralSection section = symbol.GetStructuralSection();
        bool isRectangular = section is StructuralSectionConcreteRectangle or StructuralSectionRectangleParameterized
            || (section is null or StructuralSectionUserDefined
                && (ContainsNamePart(symbol, "прямоугольн") || ContainsNamePart(symbol, "rectangular")));
        if(!isRectangular) {
            return default;
        }

        if(hasSharedDimensions) {
            return (length, width);
        }

        // Встроенные размеры сечения h и b в осях семейства колонны.
        return section switch {
            StructuralSectionConcreteRectangle rectangle => (rectangle.Height, rectangle.Width),
            StructuralSectionRectangleParameterized rectangle => (rectangle.Height, rectangle.Width),
            _ => (
                symbol.GetParamValueOrDefault<double>(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, 0),
                symbol.GetParamValueOrDefault<double>(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, 0))
        };
    }

    /// <summary>
    /// Возвращает нижнюю центральную точку в координатах текущего проекта
    /// </summary>
    public XYZ GetLocation() {
        if(PylonInstance is not null) {
            XYZ locationPoint = ((LocationPoint) PylonInstance.Location).Point;

            ElementId downLevelId = PylonInstance.GetParamValue<ElementId>(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
            Level downLevel = PylonInstance.Document.GetElement(downLevelId) as Level;

            double downLevelMark = downLevel.ProjectElevation;
            double downOffset = PylonInstance.GetParamValue<double>(
                BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);

            var columnLocation = new XYZ(
                locationPoint.X,
                locationPoint.Y,
                downLevelMark + downOffset);

            return _transform.OfPoint(columnLocation);
        } else {
            return null;
        }
    }

    private static bool ContainsNamePart(FamilySymbol symbol, string namePart) {
        return symbol.FamilyName.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0
            || symbol.Name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
