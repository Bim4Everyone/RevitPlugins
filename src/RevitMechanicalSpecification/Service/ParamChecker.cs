using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class ParamChecker {
        private readonly List<RevitParam> _revitParams = new List<RevitParam>() {
            SharedParamsConfig.Instance.VISHvacSystemFunction,
            SharedParamsConfig.Instance.VISSystemShortName,
            SharedParamsConfig.Instance.VISOutSystemName,

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
        private SpecConfiguration _specConfiguration;
        private Document _document;

        /// <summary>
        /// Если параметр в сведениях о проекте пустой - ставим стандартное значение из согласованных инженерами
        /// </summary>
        /// <param name="paraName"></param>
        /// <param name="defValue"></param>
        private void FillInfoParamIfEmpty(string paraName, double defValue) {
            double value = _document.ProjectInformation.GetSharedParamValueOrDefault<double>(paraName, 0);
            if(value == 0) {
                _document.ProjectInformation.GetSharedParam(paraName).Set(defValue);
            } 
        }

        /// <summary>
        /// Отдельная транзакция на проверку стандартных значений параметров
        /// </summary>
        private void CheckParamterValues() {
            using(var t = _document.StartTransaction("Установка стандартных значений параметров")) {
                // Цифры  по запасам получены от Денисенко Юрия, согласованы Карамовым, Воробьевым и Копысовым 
                // письмом от 08.08.2024 на основании опыта стройки
                FillInfoParamIfEmpty(_specConfiguration.ParamNameDuctInsulationStock, 20);
                FillInfoParamIfEmpty(_specConfiguration.ParamNameDuctPipeStock, 5);
                FillInfoParamIfEmpty(_specConfiguration.ParamNamePipeInsulationStock, 5);

                t.Commit();
            }
        }

        /// <summary>
        /// Создать недостающие параметры, устранить расхождения по галочкам
        /// </summary>
        /// <param name="document"></param>
        public void ExecuteParamCheck(Document document, SpecConfiguration specConfiguration) {
            _document = document;
            _specConfiguration = specConfiguration;
            ProjectParameters projectParameters = ProjectParameters.Create(document.Application);
            //projectParameters.SetupRevitParams(document, _revitParams);

            // Если использовать SetupRevitParams для всего листа сразу - любые снятые/добавленные галочки перебьются.
            // Это плохо играет на кастомных категориях
            foreach(RevitParam revitParam in _revitParams) {
                if(!document.IsExistsParam(revitParam.Name)){
                    projectParameters.SetupRevitParam(document, revitParam);
                }
            }

            CheckParamterValues();
        }
    }
}
