using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface ICategoryAvailabilityService {
    bool IsParamAvailableInCategory(RevitParam param, Category category);
}
