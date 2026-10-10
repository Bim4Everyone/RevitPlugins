using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Utilites;
internal static class SolidUtility {
    // Старт для тестовой экструзии
    private const double _startDefault = 0;
    // Финиш для тестовой экструзии
    private const double _finishDefault = 1;
    // Направление экструзии - вверх
    private static readonly XYZ _directionUp = new(0, 0, 10);
    // Направление экструзии - вниз
    private static readonly XYZ _directionDown = new(0, 0, -10);
    
    /// <summary>
    /// Получение плоскости разделения на основе грани.
    /// </summary>
    /// <remarks>
    /// В данном методе определяется нормаль и начало координат заданной грани.
    /// Для планарной грани используются ее нормаль и начало координат,
    /// для непланарной грани нормаль и точка вычисляются в центре ограничивающего
    /// прямоугольника. Полученные значения преобразуются в систему координат хоста
    /// и используются для создания плоскости разделения.
    /// </remarks>
    /// <param name="face">Грань, на основе которой создается плоскость разделения.</param>
    /// <returns>
    /// Плоскость разделения, соответствующая геометрии заданной грани.
    /// </returns>
    public static DividePlane GetPlaneFromFace(Face face) {
        XYZ normal;
        if(face is PlanarFace pf) {
            normal = pf.FaceNormal;
        } else {
            var bbox = face.GetBoundingBox();
            var uv = (bbox.Min + bbox.Max) / 2;
            normal = face.ComputeNormal(uv);
        }

        XYZ originLocal;
        if(face is PlanarFace ppf) {
            originLocal = ppf.Origin;
        } else {
            var bbox = face.GetBoundingBox();
            var uv = (bbox.Min + bbox.Max) / 2;
            originLocal = face.Evaluate(uv);
        }
        var transform = Transform.Identity;
        
        var normalHost = transform.OfVector(normal).Normalize();
        var originHost = transform.OfPoint(originLocal);

        return CreateDividePlane(normalHost, originHost);
    }
    
    /// <summary>
    /// Безопасное разделение солида плоскостью.
    /// </summary>
    /// <remarks>
    /// В данном методе из исходного солида удаляется часть, расположенная
    /// за заданной плоскостью, с использованием операции отсечения полупространством.
    /// Если операция завершается с ошибкой, возвращается <c>null</c>.
    /// </remarks>
    /// <param name="solid">Исходный солид.</param>
    /// <param name="plane">Плоскость, используемая для отсечения солида.</param>
    /// <returns>
    /// Солид, полученный после отсечения исходного солида плоскостью,
    /// или <c>null</c> в случае ошибки выполнения операции.
    /// </returns>
    public static Solid DivideSolidSafe(Solid solid, Plane plane) {
        try {
            return BooleanOperationsUtils.CutWithHalfSpace(solid, plane);
        } catch {
            return null;
        }
    }

    /// <summary>
    /// Пересечение двух солидов.
    /// </summary>
    /// <remarks>
    /// В данном методе выполняется булева операция пересечения двух солидов.
    /// Если результат пересечения отсутствует или его объем меньше допустимой геометрической погрешности,
    /// возвращается <c>null</c>.
    /// </remarks>
    /// <param name="solid1">Первый исходный солид.</param>
    /// <param name="solid2">Второй исходный солид.</param>
    /// <param name="operationsType">Тип булевой операции</param>
    /// <param name="result"></param>
    /// <returns>
    /// Солид, полученный в результате булевой операции исходных солидов,
    /// или <c>null</c>, если пересечение отсутствует, имеет недостаточный объем
    /// либо при выполнении операции произошла ошибка.
    /// </returns>
    public static bool TryGetBooleanSolid(Solid solid1, Solid solid2, BooleanOperationsType operationsType, out Solid result) {
        result = null;
        try {
            var solid = BooleanOperationsUtils.ExecuteBooleanOperation(
                solid1,
                solid2,
                operationsType);

            result = IsValid(solid) 
                ? solid 
                : null;
            return true;
        } catch (Autodesk.Revit.Exceptions.InvalidOperationException) {
            return false;
        }
    }

    /// <summary>
    /// Метод экструзии объемных элементов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится экструзия объемных элементов по заданным параметрам старта и финиша экструзии.
    /// </remarks>
    /// <param name="listCurveLoops">Замкнутый контур для экструзии.</param>
    /// <param name="start">Старт экструзии. По умолчанию 0.</param>
    /// <param name="finish">Финиш экструзии. По умолчанию 1.</param>
    /// <param name="up">Направление экструзии. По умолчанию вверх - True</param>
    /// <returns>
    /// Solid.
    /// </returns>
    public static Solid ExtrudeSolid(
        List<CurveLoop> listCurveLoops,
        double start = _startDefault,
        double finish = _finishDefault,
        bool up = true) {

        if(double.IsNaN(start) || double.IsNaN(finish)) {
            return null;
        }

        var direction = up ? _directionUp : _directionDown;

        double amountToExtrude = finish - start;

        if(listCurveLoops.Count == 0 && amountToExtrude <= GeometryTolerance.Model) {
            return null;
        }

        try {
            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(listCurveLoops, direction, amountToExtrude);
            return IsValid(solid)
                ? solid 
                : null;
        } catch {
            return null;
        }
    }

    /// <summary>
    /// Метод перемешивания солидов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится перемешивание солидов по GUID.
    /// </remarks>    
    /// <param name="solids">Исходный список солидов</param>
    /// <returns>
    /// Перемешанный список солидов.
    /// </returns>
    public static List<Solid> ShuffleSolidsByGuid(IEnumerable<Solid> solids) {
        return [.. solids.OrderBy(_ => Guid.NewGuid())];
    }

    /// <summary>
    /// Метод безопасного получения общего объема солидов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится вычисление общего объема солидов.
    /// </remarks>    
    /// <param name="solids">Исходный список солидов</param>
    /// <returns>
    /// Сумма объема всех солидов.
    /// </returns>
    public static double GetSolidsVolume(IList<Solid> solids) {
        return solids
            .Select(GetSafeSolidVolume)
            .Sum();
    }
    
    /// <summary>
    /// Метод проверки двух солидов на пересечение.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка двух солидов на пересечение.
    /// </remarks>    
    /// <param name="solid1">Первый солид для проверки</param>
    /// <param name="solid2">Второй солид для проверки</param>
    /// <returns>
    /// True - если солиды пересекаются. False - если солиды не пересекаются.
    /// </returns>
    public static bool IsIntersect(Solid solid1, Solid solid2) {
        if (solid1 is null || solid2 is null)
            return false;
        TryGetBooleanSolid(solid1, solid2, BooleanOperationsType.Intersect, out var result);
        return result != null;
    }
       
    
    /// <summary>
    /// Проверка корректности солида.
    /// </summary>
    /// <remarks>
    /// В данном методе проверяется наличие солида, его положительный объем
    /// и наличие хотя бы одной грани.
    /// </remarks>
    /// <param name="solid">Солид, подлежащий проверке.</param>
    /// <returns>
    /// <c>true</c>, если солид существует, имеет положительный объем и содержит грани;
    /// в противном случае — <c>false</c>.
    /// </returns>
    public static bool IsValid(Solid solid) {
        return solid != null && GetSafeSolidVolume(solid) > 0 && solid.Faces.Size > 0;
    }
    
    // Метод безопасного получения объёма солида
    private static double GetSafeSolidVolume(Solid solid) {
        return solid?.Volume ?? 0;
    }
    
    // Метод построения разрезающих плоскостей
    private static DividePlane CreateDividePlane(XYZ normal, XYZ origin) {
        var positivePlane = Plane.CreateByNormalAndOrigin(normal, origin);
        var negativePlane = Plane.CreateByNormalAndOrigin(normal.Negate(), origin);
        return new DividePlane { PositivePlane = positivePlane, NegativePlane = negativePlane };
    }
}
