using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;
internal class Intersector {

    public bool HasIntersection { get; private set; }

    /// <summary>
    /// Основной метод пересечения
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и элемента основного файла
    /// </remarks>
    /// <returns>Возвращает RevitElement, который пересекся с элементом модели</returns>   
    public RevitElement Intersect(Solid sphere, ICollection<RevitElement> sourceModels) {
        foreach(var sourceModel in sourceModels) {
            try {
                var resultSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                    sourceModel.Solid,
                    sphere,
                    BooleanOperationsType.Intersect);

                if(resultSolid != null && resultSolid.Volume > 0.0001) {
                    HasIntersection = true;
                    return sourceModel;
                }
            } catch {
                return null;
            }
        }
        return null;
    }
}
