using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using dosymep.WPF.ViewModels;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class DesignTypeListVM : BaseViewModel {

        private List<DesignTypeVM> _designTypes = new List<DesignTypeVM>();

        public DesignTypeListVM() { }

        public List<DesignTypeVM> DesignTypes {
            get => _designTypes;
            set => this.RaiseAndSetIfChanged(ref _designTypes, value);
        }



        public void GetInfo(bool calcСoefficientOnAverage) {
            List<DesignTypeVM> selectedDesignTypes = DesignTypes.Where(o => o.IsCheck).ToList();
            //ReportVM report = new ReportVM(_revitRepository);

            foreach(DesignTypeVM designType in selectedDesignTypes) {
                //// Проверка элементов выбранного типа конструкции
                //if(!designType.FormParamsChecked) {
                //    _typeAnalyzer.CheckParamsInFormElements(designType, report);
                //}
                //if(!designType.RebarParamsChecked) {
                //    _typeAnalyzer.CheckParamsInRebars(designType, report);
                //}

                // Если есть ошибки либо в опалубке, либо в арматуре подсчет выполняться не будет, т.к. нужны оба
                if(!designType.HasErrors && !designType.AlreadyCalculated) {
                    // Выполняем расчет объема опалубки
                    designType.CalculateFormworks();
                    // Выполняем расчет массы арматуры
                    designType.CalculateRebars();
                }
            }

            // В зависимости от выбора пользователя, рассчитываем коэффициент усредненно или по отдельности
            if(calcСoefficientOnAverage) {
                if(selectedDesignTypes.All(o => !o.HasErrors)) {
                    CalculateRebarCoefBySeveral(selectedDesignTypes);
                }
            } else {
                foreach(DesignTypeVM designType in selectedDesignTypes) {
                    if(!designType.HasErrors) {
                        designType.CalculateRebarCoef();
                    }
                }
            }
            //if(report.ReportItems.Count() > 0) {
            //    ReportWindow reportWindow = new ReportWindow(report);
            //    reportWindow.ShowDialog();
            //}
        }




        /// <summary>
        /// Рассчитывает коэффициент армирования у нескольких типов конструкции по массе арматуры и объему бетона
        /// </summary>
        public void CalculateRebarCoefBySeveral(List<DesignTypeVM> typesInfo) {
            double totalRebarMass = 0;
            double totalConcreteVolume = 0;

            // Получаем суммарный объем бетона опалубки и суммарную массу арматуры
            foreach(DesignTypeVM typeInfo in typesInfo) {
                totalRebarMass += typeInfo.RebarMass;
                totalConcreteVolume += typeInfo.FormworkVolume;
            }
            // Рассчитываем коэф армирования
            string averageReinforcementCoefficient = Math.Round(totalRebarMass / totalConcreteVolume).ToString(CultureInfo.GetCultureInfo("ru-Ru"));

            // Записываем рассчитанный коэф армирования в типы конструкций, по которым считали
            foreach(DesignTypeVM typeInfo in typesInfo) {
                typeInfo.RebarCoef = averageReinforcementCoefficient;
                typeInfo.AlreadyCalculated = true;
            }
        }
    }
}
