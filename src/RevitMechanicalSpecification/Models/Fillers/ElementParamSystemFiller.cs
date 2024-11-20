using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamSystemFiller : ElementParamFiller {
        private readonly List<VisSystem> _systemList;
        private readonly SystemFunctionNameFactory _nameFactory;
        private readonly HashSet<BuiltInCategory> _insulationCategories;
        private readonly string _tempSharedNameName = "ADSK_Имя системы";

        public ElementParamSystemFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document,
            List<VisSystem> systemList) : 
            base(toParamName, fromParamName, specConfiguration, document) 
            { 
            _insulationCategories = new HashSet<BuiltInCategory>() { 
            BuiltInCategory.OST_DuctInsulations,
            BuiltInCategory.OST_PipeInsulations
            };
            _systemList = systemList;
            _nameFactory = new SystemFunctionNameFactory(Document, _systemList, specConfiguration);
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            string calculatedSystem = GetSystemName(specificationElement);

            if(!(string.IsNullOrWhiteSpace(calculatedSystem))) {
                TargetParam.Set(calculatedSystem);
            } else {
                calculatedSystem = Config.GlobalSystem;
                TargetParam.Set(calculatedSystem);
            }

            // Параметр ADSK_Имя системы не имеет отношения к платформе, но очень часто участвует в фильтрах
            // Он будет устраняться из всех шаблонов, включая шаблоны самой B4E. Но пока он глубоко интегрирован в документацию, 
            // будем обновлять, чтоб не плодить переработки. 
            if(specificationElement.Element.IsExistsParam(_tempSharedNameName)) {
                Parameter adsk_name = specificationElement.Element.GetParam(_tempSharedNameName);

                // В основном ситуация с рид-онли встречается у внешних пользователей, наши не блокируют этот параметр
                if(!adsk_name.IsReadOnly) {
                    adsk_name.Set(calculatedSystem);
                }
            }
        }

        /// <summary>
        /// Получает имя системы или принудительное имя системы
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetSystemName(SpecificationElement specificationElement) {
            string forcedSystem = _nameFactory.GetForcedParamValue(specificationElement, Config.ForcedSystemName);
            if(!string.IsNullOrEmpty(forcedSystem)) {
                return forcedSystem;
            }

            // Если у элемента есть хост изоляции - проверяем на принудительное имя еще и его,
            // если заполнено - должно быть одно
            if(specificationElement.InsulationSpHost != null) {
                string forcedHostSystem = _nameFactory.GetForcedParamValue(specificationElement.InsulationSpHost,
                    Config.ForcedSystemName);
                if(!string.IsNullOrEmpty(forcedHostSystem)) {
                    return forcedHostSystem;
                }
            }

            return _nameFactory.GetSystemNameValue(specificationElement.Element);
        }
    }
}
