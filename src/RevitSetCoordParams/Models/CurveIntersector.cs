using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;
internal class CurveIntersector {
    private readonly ICollection<RevitElement> _sourceModels;

    public CurveIntersector(ICollection<RevitElement> sourceModels) {
        _sourceModels = sourceModels;
    }
    /// <summary>
    /// Основной метод пересечения
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и элемента основного файла
    /// </remarks>
    /// <returns>Возвращает RevitElement, который пересекся с элементом модели</returns> 
    public RevitElement IntersectWithCurves(List<Curve> curves) {
        foreach(var curve in curves) {
            var sourceModel = IntersectWithCurve(curve);
            if(sourceModel != null) {
                return sourceModel;
            }
        }
        return null;
    }
    /// <summary>
    /// Основной метод пересечения поисковой сферы
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и поисковой сферы
    /// </remarks>
    /// <returns>Возвращает RevitElement, который пересекся с поисковой сферой</returns> 
    public RevitElement IntersectWithCurve(Curve curve) {
        var options = new SolidCurveIntersectionOptions();
        foreach(var sourceModel in _sourceModels) {
            var solid = sourceModel.Solid;
            var result = solid.IntersectWithCurve(curve, options);
            if(result.ResultType == SolidCurveIntersectionMode.CurveSegmentsInside) {
                if(result.Any(item => item.Length > 0)) {
                    return sourceModel;
                }
            }
        }
        return null;
    }
}
