using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class ParamChecker {
        private readonly List<RevitParam> _paramsToDataGroup = new List<RevitParam>() {
            SharedParamsConfig.Instance.VISHvacSystemFunction,
            SharedParamsConfig.Instance.VISSystemShortName,
            SharedParamsConfig.Instance.VISOutSystemName,

            SharedParamsConfig.Instance.VISNote,
            SharedParamsConfig.Instance.VISMass,
            SharedParamsConfig.Instance.VISPosition,
            SharedParamsConfig.Instance.VISGrouping,
            SharedParamsConfig.Instance.EconomicFunction,
            SharedParamsConfig.Instance.VISSystemName,
            SharedParamsConfig.Instance.VISCombinedName,
            SharedParamsConfig.Instance.VISMarkNumber,
            SharedParamsConfig.Instance.VISItemCode,
            SharedParamsConfig.Instance.VISUnit,
            SharedParamsConfig.Instance.VISManufacturer,

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
            SharedParamsConfig.Instance.VISExcludeFromJunction,
            SharedParamsConfig.Instance.VISSpecNumbersCurrency,
            SharedParamsConfig.Instance.VISSpecNumbers
        };
        private readonly List<RevitParam> _paramsToConstraints = new List<RevitParam>() {
            SharedParamsConfig.Instance.VISNameAddition,
            SharedParamsConfig.Instance.VISNameForced,
            SharedParamsConfig.Instance.VISSystemNameForced,
            SharedParamsConfig.Instance.VISGroupingForced,
            SharedParamsConfig.Instance.VISHvacSystemForcedFunction,
        };
        private readonly List<RevitParam> _revitParams = new List<RevitParam>() {
            SharedParamsConfig.Instance.VISHvacSystemFunction,
            SharedParamsConfig.Instance.VISSystemShortName,
            SharedParamsConfig.Instance.VISOutSystemName,

            SharedParamsConfig.Instance.VISNote,
            SharedParamsConfig.Instance.VISMass,
            SharedParamsConfig.Instance.VISPosition,
            SharedParamsConfig.Instance.VISGrouping,
            SharedParamsConfig.Instance.EconomicFunction,
            SharedParamsConfig.Instance.VISSystemName,
            SharedParamsConfig.Instance.VISCombinedName,
            SharedParamsConfig.Instance.VISMarkNumber,
            SharedParamsConfig.Instance.VISItemCode,
            SharedParamsConfig.Instance.VISUnit,
            SharedParamsConfig.Instance.VISManufacturer,

            SharedParamsConfig.Instance.VISNameAddition,
            SharedParamsConfig.Instance.VISNameForced,
            SharedParamsConfig.Instance.VISSystemNameForced,
            SharedParamsConfig.Instance.VISGroupingForced,
            SharedParamsConfig.Instance.VISHvacSystemForcedFunction,

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
                // Цифры  по запасам получены получены письмом от 08.08.2024 на основании опыта стройки
                FillInfoParamIfEmpty(_specConfiguration.ParamNameDuctInsulationStock, 20);
                FillInfoParamIfEmpty(_specConfiguration.ParamNameDuctPipeStock, 5);
                FillInfoParamIfEmpty(_specConfiguration.ParamNamePipeInsulationStock, 5);

                t.Commit();
            }
        }

        // Если параметр существует - назначает его в указанную группу. Работы с шаблоном недостаточно, параметры в итоге оказываются в произвольных группах
        private void SortParameterToGroup(Document document, string paraname, ForgeTypeId group) {
            SharedParameterElement param = document.GetSharedParam(paraname);
            if(param is null) {
                return;
            }
            InternalDefinition definition = param.GetDefinition();

            definition.ReInsertToGroup(document, group);
        }

        // Временная проверка пока мы не переходим на редактируемые экземпляры групп. Если ФОП_ВИС_Число и ФОП_ВИС_Число ДЕ в проекте вместе - отменяем работу 
        private void CheckNumberDuplicate(Document document) {
            if(document.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbers) &&
                (document.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbersCurrency))) {

                MessageBox.Show(
                    "Работа невозможна при одновременном наличии в проекте параметров ФОП_ВИС_Число и ФОП_ВИС_Число ДЕ");
                throw new OperationCanceledException();
            }

            if(!document.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbersCurrency)) {
                _revitParams.Add(SharedParamsConfig.Instance.VISSpecNumbers);
            } else {
                _revitParams.Add(SharedParamsConfig.Instance.VISSpecNumbersCurrency);
            }
        }

        /// <summary>
        /// Создать недостающие параметры, устранить расхождения по галочкам
        /// </summary>
        /// <param name="document"></param>
        public void ExecuteParamCheck(Document document, SpecConfiguration specConfiguration) {
            _document = document;
            _specConfiguration = specConfiguration;

            CheckNumberDuplicate(document);
            ProjectParameters projectParameters = ProjectParameters.Create(document.Application);
            projectParameters.SetupRevitParams(document, _revitParams);

            using(var t = _document.StartTransaction("Смена групп параметров")) {
                foreach(RevitParam revitParam in _paramsToDataGroup) {
                    SortParameterToGroup(document, revitParam.Name, GroupTypeId.Data);
                }

                foreach(RevitParam revitParam in _paramsToConstraints) {
                    SortParameterToGroup(document, revitParam.Name, GroupTypeId.Constraints);
                }
                t.Commit();
            }

            CheckParamterValues();
        }
    }
}
