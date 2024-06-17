using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models.ElementModels;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class DesignTypeListVM : BaseViewModel {

        private List<DesignTypeVM> _designTypes = new List<DesignTypeVM>();
        private readonly MainViewModel _mvm;

        public DesignTypeListVM(MainViewModel mvm) {
            _mvm = mvm;
            SetFiltering();
        }

        public List<DesignTypeVM> DesignTypes {
            get => _designTypes;
            set => this.RaiseAndSetIfChanged(ref _designTypes, value);
        }


        /// <summary>
        /// Используется в качестве аргумента предиката для фильтрации списка по выбранному комплекту документации
        /// </summary>
        public bool FilterByDocPackage(object o) {
            if(_mvm.SelectedDockPackage == _mvm.FilterValueForNoFiltering) {
                return true;
            }

            // Если в параметре есть какое то значение (не null и не пустая строка (у нас это тоже null))
            if(string.IsNullOrEmpty(_mvm.SelectedDockPackage)) {
                return string.IsNullOrEmpty(((DesignTypeVM) o).DocPackage);
            } else {
                return ((DesignTypeVM) o).DocPackage is null ? false : ((DesignTypeVM) o).DocPackage.Equals(_mvm.SelectedDockPackage);
            }
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
        /// Запись значений коэффициенто армирования
        /// </summary>
        public void WriteRebarCoef(Document doc) {
            foreach(DesignTypeVM designType in DesignTypes.Where(o => o.IsCheck)) {
                // Если нет ошибок то выполняем запись значений коэффициента армирования в опалубочные элементы
                if(!designType.HasErrors) {
                    using(Transaction transaction = doc.StartTransaction("Запись коэффициентов армирования")) {
                        foreach(FormworkElement elem in designType.Formworks) {
                            elem.RevitElement.SetParamValue("ФОП_ТИП_Армирование", designType.RebarCoef);
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Ставит галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        public void SelectAllVisible() {
            foreach(DesignTypeVM item in DesignTypes.Where(FilterByDocPackage)) {
                item.IsCheck = true;
            }
        }

        /// <summary>
        /// Снимает галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        public void UnselectAllVisible() {
            foreach(DesignTypeVM item in DesignTypes.Where(FilterByDocPackage)) {
                item.IsCheck = false;
            }
        }

        /// <summary>
        /// Обновляем элемент в UI, привязанный к коллекции типов конструкций с учетом фильтрации
        /// </summary>
        public void UpdateFiltering() {
            CollectionViewSource.GetDefaultView(DesignTypes).Refresh();
        }

        /// <summary>
        /// Задаем фильтрацию списка типов конструкций по выбранному комплекту документации
        /// </summary>
        private void SetFiltering() {
            CollectionViewSource.GetDefaultView(DesignTypes).Filter = new Predicate<object>(FilterByDocPackage);
        }


        /// <summary>
        /// Рассчитывает коэффициент армирования у нескольких типов конструкции по массе арматуры и объему бетона
        /// </summary>
        private void CalculateRebarCoefBySeveral(List<DesignTypeVM> typesInfo) {
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
