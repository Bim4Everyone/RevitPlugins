using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IParamAvailabilityService {
    bool IsParamExist(Document docstring, string paramName);
    Definition GetDefinitionByName(Document doc, string paramName);
}
