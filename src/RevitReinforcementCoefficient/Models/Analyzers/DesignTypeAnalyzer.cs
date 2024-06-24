using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitReinforcementCoefficient.Models.ElementModels;
using RevitReinforcementCoefficient.ViewModels;

namespace RevitReinforcementCoefficient.Models.Analyzers {
    internal class DesignTypeAnalyzer {
        private readonly ParamUtils _paramUtils;

        private readonly List<string> _paramsForFormElements = new List<string>() { "ФОП_ТИП_Армирование" };

        private readonly List<string> _paramsForRebars = new List<string>() {
            "мод_ФОП_Диаметр",
            "обр_ФОП_Длина",
            "обр_ФОП_Расчет в погонных метрах",
            "обр_ФОП_Количество типовых на этаже",
            "обр_ФОП_Количество типовых этажей"
        };

        private readonly List<string> _paramsForSysRebars = new List<string>() {
            "Полная длина стержня",
            "Количество"
        };

        private readonly List<string> _paramsForIfcRebars = new List<string>() { "обр_ФОП_Количество" };


        public DesignTypeAnalyzer(ParamUtils paramUtils) {
            _paramUtils = paramUtils;
        }


        /// <summary>
        /// Проверяет наличие параметров у опалубки по типу конструкции
        /// </summary>
        public void CheckParamsInFormworks(DesignTypeVM designType) {
            foreach(ICommonElement elem in designType.Formworks) {
                if(!_paramUtils.HasParamsAnywhere(elem.RevitElement, _paramsForFormElements)) {
                    designType.HasErrors = true;
                }
            }
            designType.FormParamsChecked = true;
        }


        /// <summary>
        /// Проверяет наличие параметров у арматуры по типу конструкции
        /// </summary>
        public void CheckParamsInRebars(DesignTypeVM designType) {
            foreach(ICommonElement rebar in designType.Rebars) {
                // Далее проверяем параметры, которые должны быть у всех элементов арматуры
                if(!_paramUtils.HasParamsAnywhere(rebar.RevitElement, _paramsForRebars)) {
                    designType.HasErrors = true;
                }

                // Здесь проверяем разные параметры, которые должны быть у системной/IFC арматуры
                // Если элемент класса Rebar (т.е. системная арматура)
                if(rebar.RevitElement is FamilyInstance) {
                    if(!_paramUtils.HasParamsAnywhere(rebar.RevitElement, _paramsForIfcRebars)) {
                        designType.HasErrors = true;
                    }
                } else {
                    if(!_paramUtils.HasParamsAnywhere(rebar.RevitElement, _paramsForSysRebars)) {
                        designType.HasErrors = true;
                    }
                }
            }
            designType.RebarParamsChecked = true;
        }
    }
}
