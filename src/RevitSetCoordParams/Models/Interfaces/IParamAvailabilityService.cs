using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitSetCoordParams.Models.Interfaces;

public interface IParamAvailabilityService {
    bool IsParamAvailable(RevitParam param, Category category);
    bool IsParamExist(string paramName);
    Definition GetDefinitionByName(string paramName);
}
