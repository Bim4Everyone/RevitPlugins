using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonDocumentation.Models;
internal class ParamValueService {
    private readonly RevitRepository _revitRepository;

    public ParamValueService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }


    public T GetParamValueAnywhere<T>(Element elem, string paramName) {
        T paramValue = elem.GetParamValueOrDefault<T>(paramName, default);
        return EqualityComparer<T>.Default.Equals(paramValue, default)
            ? _revitRepository.Document.GetElement(elem.GetTypeId()).GetParamValueOrDefault<T>(paramName, default)
            : paramValue;
    }
}
