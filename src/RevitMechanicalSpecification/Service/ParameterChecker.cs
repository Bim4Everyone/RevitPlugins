using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class ParameterChecker {
        private readonly List<RevitParam> _revitParams = new List<RevitParam>() {
            SharedParamsConfig.Instance.VISGrouping,
            SharedParamsConfig.Instance.EconomicFunction,
            SharedParamsConfig.Instance.VISSystemName,
            SharedParamsConfig.Instance.VISCombinedName,
            SharedParamsConfig.Instance.VISMarkNumber,
            SharedParamsConfig.Instance.VISItemCode,
            SharedParamsConfig.Instance.VISUnit,
            SharedParamsConfig.Instance.VISManufacturer,
            SharedParamsConfig.Instance.VISSpecNumbers,
            SharedParamsConfig.Instance.VISNameAddition,
            SharedParamsConfig.Instance.VISNameForced,
            SharedParamsConfig.Instance.VISSystemNameForced,
            SharedParamsConfig.Instance.VISGroupingForced,
            SharedParamsConfig.Instance.VISEconomicFunction,
            SharedParamsConfig.Instance.VISMinDuctThickness,
            SharedParamsConfig.Instance.VISMaxDuctThickness,
            SharedParamsConfig.Instance.VISParamReplacementName,
            SharedParamsConfig.Instance.VISParamReplacementMarkNumber,
            SharedParamsConfig.Instance.VISParamReplacementItemCode,
            SharedParamsConfig.Instance.VISParamReplacementUnit,
            SharedParamsConfig.Instance.VISParamReplacementManufacturer,
            SharedParamsConfig.Instance.VISOutSystemName,
            SharedParamsConfig.Instance.VISConsiderPipeFittings,
            SharedParamsConfig.Instance.VISConsiderPipeFittingsByType,
            SharedParamsConfig.Instance.VISConsiderDuctFittings,
            SharedParamsConfig.Instance.VISPipeInsulationReserve,
            SharedParamsConfig.Instance.VISDuctInsulationReserve,
            SharedParamsConfig.Instance.VISPipeDuctReserve,
            SharedParamsConfig.Instance.VISIndividualStock,
            SharedParamsConfig.Instance.VISJunction,
            SharedParamsConfig.Instance.VISExcludeFromJunction
        };

        /// <summary>
        /// Создать недостающие параметры, устранить расхождения по галочкам
        /// </summary>
        /// <param name="document"></param>
        public void ExecuteParameterCheck(Document document) {
            ProjectParameters projectParameters = ProjectParameters.Create(document.Application);
            projectParameters.SetupRevitParams(document, _revitParams);
        }
    }
}
