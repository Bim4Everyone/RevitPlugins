using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;
internal class Intersector {

    public bool HasIntersection { get; private set; }

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
