using Autodesk.Revit.DB;

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

        PylonInstance = element;
        FacingOrientation = _transform.OfVector(((FamilyInstance) element).FacingOrientation);

        Element elementType = element.Document.GetElement(element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());

        Length = elementType.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeLength, 0);
        Width = elementType.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeWidth, 0);
        Height = element.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeHeight, 0);
    }

    public Element PylonInstance { get; }

    /// <summary>
    /// Нормаль к одной из коротких сторон пилона в координатах текущего проекта.
    /// </summary>
    public XYZ FacingOrientation { get; }

    public double Length { get; }
    public double Width { get; }
    public double Height { get; }

    /// <summary>
    /// Возвращает нижнюю центральную точку в координатах текущего проекта
    /// </summary>
    /// <returns></returns>
    public XYZ GetLocation() {
        if(PylonInstance != null) {
            XYZ locationPoint = ((LocationPoint) PylonInstance.Location).Point;

            Level downLevel = PylonInstance.Document.GetElement(PylonInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId()) as Level;

            double downLevelMark = downLevel.ProjectElevation;
            double downOffset = PylonInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();

            XYZ columnLocation = new XYZ
                (
                    locationPoint.X,
                    locationPoint.Y,
                    downLevelMark + downOffset);

            return _transform.OfPoint(columnLocation);
        } else {
            return null;
        }
    }
}
