using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services;
internal abstract class OutcomingTaskOffsetFinder<T> : IOutcomingTaskOffsetFinder where T : Element {
    protected OutcomingTaskOffsetFinder(
        OpeningConfig openingConfig,
        OutcomingTaskGeometryProvider geometryProvider,
        GeometryUtils geometryUtils,
        ILengthConverter lengthConverter) {

        OpeningConfig = openingConfig ?? throw new System.ArgumentNullException(nameof(openingConfig));
        GeometryProvider = geometryProvider ?? throw new System.ArgumentNullException(nameof(geometryProvider));
        GeometryUtils = geometryUtils ?? throw new System.ArgumentNullException(nameof(geometryUtils));
        LengthConverter = lengthConverter ?? throw new System.ArgumentNullException(nameof(lengthConverter));
    }


    protected OutcomingTaskGeometryProvider GeometryProvider { get; }

    protected GeometryUtils GeometryUtils { get; }

    protected OpeningConfig OpeningConfig { get; }

    protected ILengthConverter LengthConverter { get; }

    /// <summary>
    /// Количество точек тесселяции для изогнутых линий
    /// </summary>
    protected abstract int TessellationCount { get; }


    public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
        if(IsSpecialOrthogonalCase(opening, mepElement)) {
            return FindHorizontalOffsetsSumOrthogonal(opening, mepElement);
        } else {
            var leftPlane = GeometryProvider.GetLeftPlane(opening);
            var rightPlane = GeometryProvider.GetRightPlane(opening);
            var solid = GetIntersectionSolid(opening, mepElement);
            var points = GeometryUtils.GetPoints(solid, TessellationCount);

            double leftDist = GeometryUtils.GetMinDistance(leftPlane, points);
            double rightDist = GeometryUtils.GetMinDistance(rightPlane, points);

            return leftDist + rightDist;
        }
    }

    public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
        if(IsSpecialOrthogonalCase(opening, mepElement)) {
            return FindVerticalOffsetsSumOrthogonal(opening, mepElement);
        } else {
            var bottomPlane = GeometryProvider.GetBottomPlane(opening);
            var topPlane = GeometryProvider.GetTopPlane(opening);
            var solid = GetIntersectionSolid(opening, mepElement);
            var points = GeometryUtils.GetPoints(solid, TessellationCount);

            double bottomDist = GeometryUtils.GetMinDistance(bottomPlane, points);
            double topDist = GeometryUtils.GetMinDistance(topPlane, points);

            return bottomDist + topDist;
        }
    }

    public double GetMinHorizontalOffsetSum(Element mepElement) {
        var element = GetMepElement(mepElement);
        double offset = GetOffset(mepElement, GetWidth(element));
        double tolerance = GetTolerance(mepElement);
        return offset > tolerance ? offset - tolerance : 0;
    }

    public double GetMaxHorizontalOffsetSum(Element mepElement) {
        var element = GetMepElement(mepElement);
        double offset = GetOffset(mepElement, GetWidth(element));
        double tolerance = GetTolerance(mepElement);
        return offset + tolerance;
    }

    public double GetMinVerticalOffsetSum(Element mepElement) {
        var element = GetMepElement(mepElement);
        double offset = GetOffset(mepElement, GetHeight(element));
        double tolerance = GetTolerance(mepElement);
        return offset > tolerance ? offset - tolerance : 0;
    }

    public double GetMaxVerticalOffsetSum(Element mepElement) {
        var element = GetMepElement(mepElement);
        double offset = GetOffset(mepElement, GetHeight(element));
        double tolerance = GetTolerance(mepElement);
        return offset + tolerance;
    }

    /// <summary>
    /// Находит солид пересечения элемента ВИС с заданием
    /// </summary>
    /// <param name="opening">Задание на отверстие</param>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Солид, образованный пересечением тел</returns>
    protected Solid GetIntersectionSolid(OpeningMepTaskOutcoming opening, Element mepElement) {
        var element = GetMepElement(mepElement);
        var intersection = GetMepSolid(element);
        var frontFace = GeometryProvider.GetFrontPlane(opening);
        var backFace = GeometryProvider.GetBackPlane(opening);

        intersection = BooleanOperationsUtils.CutWithHalfSpace(intersection, frontFace);
        intersection = BooleanOperationsUtils.CutWithHalfSpace(intersection, backFace);
        return intersection;
    }

    /// <summary>
    /// Возвращает точность единицах Revit (футах)
    /// </summary>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Точность в единицах Revit</returns>
    protected double GetTolerance(Element mepElement) {
        var element = GetMepElement(mepElement);
        return LengthConverter.ConvertToInternal(GetCategory(element).Rounding);
    }

    /// <summary>
    /// Находит требуемый отступ в единицах Revit (футах) для заданного размера элемента ВИС с двух сторон
    /// </summary>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <param name="size">Размер элемента ВИС единицах Revit (футах)</param>
    /// <returns>Требуемый отступ суммарно с двух сторон от элемента ВИС единицах Revit (футах)</returns>
    protected double GetOffset(Element mepElement, double size) {
        var element = GetMepElement(mepElement);
        return GetCategory(element).GetOffsetValue(size);
    }

    /// <summary>
    /// Находит высоту элемента ВИС в единицах Revit для получения требуемых отступов
    /// </summary>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Высота в единицах Revit</returns>
    protected abstract double GetHeight(T mepElement);

    /// <summary>
    /// Находит ширину элемента ВИС в единицах Revit для получения требуемых отступов
    /// </summary>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Ширина в единицах Revit</returns>
    protected abstract double GetWidth(T mepElement);

    /// <summary>
    /// Возвращает солид элемента ВИС для получения отступов
    /// </summary>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Солид элемента ВИС</returns>
    protected abstract Solid GetMepSolid(T mepElement);

    protected abstract MepCategory GetCategory(T mepElement);

    private T GetMepElement(Element mepElement) {
        return mepElement as T;
    }

    /// <summary>
    /// Находит сумму отступов по горизонтали от элемента ВИС до габаритов задания в частном ортогональном случае
    /// </summary>
    /// <param name="opening">Задание на отверстие</param>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Сумма отступов по горизонтали в единицах Revit</returns>
    private double FindHorizontalOffsetsSumOrthogonal(OpeningMepTaskOutcoming opening, Element mepElement) {
        double openingWidth = GeometryProvider.GetWidth(opening);
        var element = GetMepElement(mepElement);
        double mepWidth = GetWidth(element);
        return openingWidth - mepWidth;
    }

    /// <summary>
    /// Находит сумму отступов по вертикали от элемента ВИС до габаритов задания в частном ортогональном случае
    /// </summary>
    /// <param name="opening">Задание на отверстие</param>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>Сумма отступов по вертикали в единицах Revit</returns>
    private double FindVerticalOffsetsSumOrthogonal(OpeningMepTaskOutcoming opening, Element mepElement) {
        double openingHeight = GeometryProvider.GetHeight(opening);
        var element = GetMepElement(mepElement);
        double mepHeight = GetHeight(element);
        return openingHeight - mepHeight;
    }

    /// <summary>
    /// Проверяет, что задание на отверстие пересекается с линейным элементом ВИС под прямым углом
    /// </summary>
    /// <param name="opening">Задание на отверстие</param>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <returns>True, если элемент ВИС пересекает задание на отверстие под прямым углом, иначе false</returns>
    private bool IsSpecialOrthogonalCase(OpeningMepTaskOutcoming opening, Element mepElement) {
        if(mepElement is MEPCurve mepCurve) {
            var frontPlane = GeometryProvider.GetRotationAsVector(opening);
            var mepDirection = ((Line) ((LocationCurve) mepCurve.Location).Curve).Direction;
            return frontPlane.CrossProduct(mepDirection).IsAlmostEqualTo(XYZ.Zero);
        } else {
            return false;
        }
    }
}
