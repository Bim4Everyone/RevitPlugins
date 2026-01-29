using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

namespace RevitTagAllCategories.Models.Filtration
{
    internal class DataProvider : IDataProvider {
        private readonly RevitRepository _revitRepository;

        public DataProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public ICollection<Category> GetCategories() {
            return _revitRepository.GetFilterableCategories().ToList();
        }

        public ICollection<Document> GetDocuments() {
            return new List<Document>() { _revitRepository.Document };
        }


        public ICollection<RevitParam> GetParams(ICollection<Category> categories) {
            return ParameterFilterUtilities
                .GetFilterableParametersInCommon(_revitRepository.Document, [.. categories.Select(c => c.Id)])
                .Select(GetFilterableParam)
                .Where(p => p != null)
                .ToArray();
        }

        private RevitParam GetFilterableParam(ElementId paramId) {
            try {
                if(paramId.IsSystemId()) {
                    return SystemParamsConfig.Instance
                        .CreateRevitParam(_revitRepository.Document, (BuiltInParameter) paramId.GetIdValue());
                }

                var element = _revitRepository.Document.GetElement(paramId);
                if(element is SharedParameterElement sharedParameterElement) {
                    SharedParamsConfig.Instance.CreateRevitParam(
                        _revitRepository.Document,
                        sharedParameterElement.Name);
                }

                if(element is ParameterElement parameterElement) {
                    return ProjectParamsConfig.Instance.CreateRevitParam(_revitRepository.Document, parameterElement.Name);
                }

                return null;
            } catch(Exception) {
                return null;
            }
        }
    }
}
