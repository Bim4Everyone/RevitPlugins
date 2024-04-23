using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.Views;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private readonly DesignTypeAnalyzer _typeAnalyzer;
        private readonly ParamUtils _paramUtils;
        private readonly CalculationUtils _сalculationUtils;

        /// <summary>
        /// Значение фильтра, когда не задана фильтрация ("<Не выбрано>")
        /// </summary>
        private readonly string _filterValueForNofiltering = "<Не выбрано>";

        private string _errorText = string.Empty;
        private List<Element> _allElements;
        private List<DesignTypeInfoVM> _designTypes = new List<DesignTypeInfoVM>();

        private List<string> _dockPackages;
        private string _selectedDockPackage;
        private bool _calcСoefficientOnAverage = false;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _paramUtils = new ParamUtils();

            _typeAnalyzer = new DesignTypeAnalyzer(_paramUtils);
            _сalculationUtils = new CalculationUtils(_paramUtils);

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            ShowFormworkElementsCommand = RelayCommand.Create(ShowFormworkElements, CanShowElements);
            ShowRebarElementsCommand = RelayCommand.Create(ShowRebarElements, CanShowElements);

            GetInfoCommand = RelayCommand.Create(GetInfo, CanShowElements);

            SelectAllVisibleCommand = RelayCommand.Create(SelectAllVisible);
            UnselectAllVisibleCommand = RelayCommand.Create(UnselectAllVisible);
        }

        public ICommand SelectAllVisibleCommand { get; }
        public ICommand UnselectAllVisibleCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand ShowFormworkElementsCommand { get; }
        public ICommand ShowRebarElementsCommand { get; }

        public ICommand GetInfoCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public List<Element> AllElements {
            get => _allElements;
            set => this.RaiseAndSetIfChanged(ref _allElements, value);
        }

        public List<DesignTypeInfoVM> DesignTypes {
            get => _designTypes;
            set => this.RaiseAndSetIfChanged(ref _designTypes, value);
        }

        public List<string> DockPackages {
            get => _dockPackages;
            set => this.RaiseAndSetIfChanged(ref _dockPackages, value);
        }

        public string SelectedDockPackage {
            get => _selectedDockPackage;
            set {
                _selectedDockPackage = value;
                CollectionViewSource.GetDefaultView(DesignTypes).Refresh();
                RaisePropertyChanged("SelectedDockPackage");
            }
        }

        public bool СalcСoefficientOnAverage {
            get => _calcСoefficientOnAverage;
            set => this.RaiseAndSetIfChanged(ref _calcСoefficientOnAverage, value);
        }



        private void LoadView() {
            LoadConfig();

            AllElements = _revitRepository.ElementsByFilterInActiveView;

            ReportVM report = new ReportVM(_revitRepository);
            DesignTypes = _typeAnalyzer.CheckNSortByDesignTypes(AllElements, report);

            DockPackages = DesignTypes.Select(o => o.DocPackage).Distinct().OrderBy(o => o).ToList();
            DockPackages.Insert(0, _filterValueForNofiltering);
            SelectedDockPackage = DockPackages.FirstOrDefault();

            CollectionViewSource.GetDefaultView(DesignTypes).Filter = new Predicate<object>(FilterByDocPackage);

            if(report.ReportItems.Count() > 0) {

                ReportWindow reportWindow = new ReportWindow(report);
                reportWindow.ShowDialog();
            }
        }

        private void AcceptView() {
            SaveConfig();

            WriteRebarCoef();
        }

        private bool CanAcceptView() {

            if(DesignTypes.Count == 0) {
                ErrorText = "Не удалось отобрать элементы";
                return false;
            }

            if(DesignTypes.FirstOrDefault(o => o.IsCheck) is null) {
                ErrorText = "Не выбран ни один тип конструкции";
                return false;
            }

            if(DesignTypes.FirstOrDefault(o => o.IsCheck && o.AlreadyCalculated) is null) {
                ErrorText = "Не рассчитан ни один выбранный тип конструкции";
                return false;
            }

            if(!DesignTypes.Where(o => o.IsCheck).All(o => o.AlreadyCalculated)) {
                ErrorText = "Не рассчитан один из выбранных типов конструкции";
                return false;
            }

            ErrorText = string.Empty;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            _pluginConfig.SaveProjectConfig();
        }


        private void ShowFormworkElements() {

            List<ElementId> ids = new List<ElementId>();

            foreach(DesignTypeInfoVM designType in DesignTypes.Where(o => o.IsCheck)) {

                ids.AddRange(designType.Elements.Select(e => e.Id));
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(ids);
        }


        private void ShowRebarElements() {

            List<ElementId> ids = new List<ElementId>();

            foreach(DesignTypeInfoVM designType in DesignTypes.Where(o => o.IsCheck)) {

                ids.AddRange(designType.Rebars.Select(e => e.Id));
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(ids);
        }

        private bool CanShowElements() {

            return DesignTypes.FirstOrDefault(o => o.IsCheck) is null ? false : true;
        }


        /// <summary>
        /// Получение инфорнмации об объеме опалубки, массе армирования и коэффициенте армирования 
        /// у выбранных типах конструкции
        /// </summary>
        private void GetInfo() {

            if(DesignTypes.FirstOrDefault(o => o.IsCheck) is null) {
                TaskDialog.Show("Ошибка!", "Выберите тип конструкции!");
                return;
            }

            IEnumerable<DesignTypeInfoVM> selectedDesignTypes = DesignTypes.Where(o => o.IsCheck);

            ReportVM report = new ReportVM(_revitRepository);

            foreach(DesignTypeInfoVM designType in selectedDesignTypes) {

                // Проверка элементов выбранного типа конструкции
                if(!designType.FormParamsChecked) {

                    _typeAnalyzer.CheckParamsInFormElements(designType, report);
                }
                if(!designType.RebarParamsChecked) {

                    _typeAnalyzer.CheckParamsInRebars(designType, report);
                }

                // Если есть ошибки либо в опалубке, либо в арматуре подсчет выполняться не будет, т.к. нужны оба
                if(!designType.HasErrors && !designType.AlreadyCalculated) {

                    // Выполняем расчет объема опалубки
                    _сalculationUtils.CalculateConcreteVolume(designType);
                    // Выполняем расчет массы арматуры
                    _сalculationUtils.CalculateRebarMass(designType, report);
                }
            }

            // В зависимости от выбора пользователя, рассчитываем коэффициент усредненно или по отдельности
            if(СalcСoefficientOnAverage) {

                if(selectedDesignTypes.All(o => !o.HasErrors)) {

                    _сalculationUtils.CalculateRebarCoefBySeveral(selectedDesignTypes);
                }
            } else {

                foreach(DesignTypeInfoVM designType in selectedDesignTypes) {

                    if(!designType.HasErrors) {

                        _сalculationUtils.CalculateRebarCoef(designType);
                    }
                }
            }

            if(report.ReportItems.Count() > 0) {

                ReportWindow reportWindow = new ReportWindow(report);
                reportWindow.ShowDialog();
            }
        }


        /// <summary>
        /// Запись значений коэффициенто армирования
        /// </summary>
        private void WriteRebarCoef() {

            if(DesignTypes.FirstOrDefault(o => o.IsCheck) is null) {
                TaskDialog.Show("Ошибка!", "Выберите тип конструкции!");
                return;
            }

            foreach(DesignTypeInfoVM designType in DesignTypes.Where(o => o.IsCheck)) {

                // Если нет ошибок то выполняем запись значений коэффициента армирования в опалубочные элементы
                if(!designType.HasErrors) {

                    using(Transaction transaction = _revitRepository.Document.StartTransaction("Запись коэффициентов армирования")) {
                        foreach(Element elem in designType.Elements) {

                            elem.SetParamValue("ФОП_ТИП_Армирование", designType.RebarCoef.ToString());
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Ставит галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        private void SelectAllVisible() {

            foreach(DesignTypeInfoVM item in DesignTypes.Where(FilterByDocPackage)) {

                item.IsCheck = true;
            }
        }

        /// <summary>
        /// Снимает галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        private void UnselectAllVisible() {

            foreach(DesignTypeInfoVM item in DesignTypes.Where(FilterByDocPackage)) {

                item.IsCheck = false;
            }
        }


        /// <summary>
        /// Используется в качестве аргумента предиката для фильтрации списка по выбранному комплекту документации
        /// </summary>
        private bool FilterByDocPackage(object o) {
            // Если в параметре есть какое то значение (не null и не пустая строка (у нас это тоже null))
            if(SelectedDockPackage != _filterValueForNofiltering) {

                if(string.IsNullOrEmpty(SelectedDockPackage)) {

                    return string.IsNullOrEmpty(((DesignTypeInfoVM) o).DocPackage) ? true : false;
                } else {

                    return ((DesignTypeInfoVM) o).DocPackage is null ? false : ((DesignTypeInfoVM) o).DocPackage.Contains(SelectedDockPackage);
                }
            }

            return true;
        }
    }
}
