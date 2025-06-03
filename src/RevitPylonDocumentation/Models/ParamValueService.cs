using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonDocumentation.Models {
    internal class ParamValueService {
        private readonly RevitRepository _revitRepository;

        public ParamValueService(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }


        public int GetParamValueAnywhere(Element elem, string paramName) {
            var paramValue = elem.GetParamValueOrDefault<int>(paramName, 0);
            return paramValue == 0
                ? _revitRepository.Document.GetElement(elem.GetTypeId()).GetParamValueOrDefault<int>(paramName, 0)
                : paramValue;
        }
    }
}
