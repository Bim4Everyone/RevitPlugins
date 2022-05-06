using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class ParameterInitializer {
        private readonly RevitRepository _revitRepository;

        public ParameterInitializer(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public RevitParam InitializeParameter(ElementId id) {
            if(id.IntegerValue < 0) {
#if D2020 || R2020 || D2021 || R2021
                return SystemParamsConfig.Instance.CreateRevitParam((BuiltInParameter) id.IntegerValue);
#elif D2022 || R2022 

#endif
            } else {
                var element = _revitRepository.Getelement(id);
                if (element is SharedParameterElement sharedParameterElement) {
                    return SharedParamsConfig.Instance.CreateRevitParam(_revitRepository.Doc, sharedParameterElement.Name);
                }
                if(element is ParameterElement parameterElement) {
                    return ProjectParamsConfig.Instance.CreateRevitParam(_revitRepository.Doc, parameterElement.Name);
                }
            }
            throw new ArgumentException(nameof(id), $"Невозможно преобразовать в параметр элемент с id - {id}.");
        }
    }
}
