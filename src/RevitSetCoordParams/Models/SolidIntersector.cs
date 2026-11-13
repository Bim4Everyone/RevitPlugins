using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;
internal class SolidIntersector {
    private readonly ICollection<RevitElement> _sourceModels;

    public SolidIntersector(ICollection<RevitElement> sourceModels) {
        _sourceModels = sourceModels;
    }
    /// <summary>
    /// Основной метод пересечения
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и элемента основного файла
    /// </remarks>
    /// <returns>Возвращает RevitElement, который пересекся с элементом модели</returns>   
    public RevitElement Intersect(Solid sphere) {
        foreach(var sourceModel in _sourceModels) {
            try {
                var resultSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    sourceModel.Solid,
                    sphere,
                    BooleanOperationsType.Intersect);

                if(resultSolid != null && resultSolid.Volume > 0) {
                    return sourceModel;
                }
            } catch {
                return null;
            }
        }
        return null;
    }
}
