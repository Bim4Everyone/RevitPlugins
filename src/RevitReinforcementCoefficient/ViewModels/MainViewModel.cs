using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.Views;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private readonly DesignTypeAnalyzer _typeAnalyzer;

        /// <summary>
        /// Значение фильтра, когда не задана фильтрация ("<Не выбрано>")
        /// </summary>
        private readonly string _filterValueForNofiltering = "<Не выбрано>";

        private string _errorText = string.Empty;
        private List<Element> _allElements;
        private DesignTypeListVM _designTypesList = new DesignTypeListVM();

        private List<string> _dockPackages;
        private string _selectedDockPackage;
        private bool _calcСoefficientOnAverage = false;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _typeAnalyzer = new DesignTypeAnalyzer();

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

        //public List<DesignTypeInfoVM> DesignTypes {
        //    get => _designTypes;
        //    set => this.RaiseAndSetIfChanged(ref _designTypes, value);
        //}

        public DesignTypeListVM DesignTypesList {
            get => _designTypesList;
            set => this.RaiseAndSetIfChanged(ref _designTypesList, value);
        }

        public List<string> DockPackages {
            get => _dockPackages;
            set => this.RaiseAndSetIfChanged(ref _dockPackages, value);
        }

        public string SelectedDockPackage {
            get => _selectedDockPackage;
            set {
                _selectedDockPackage = value;
                //CollectionViewSource.GetDefaultView(DesignTypes).Refresh();
                //RaisePropertyChanged(nameof(SelectedDockPackage));
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

            DesignTypesList.DesignTypes = _typeAnalyzer.CheckNSortByDesignTypes(AllElements, report);

            DockPackages = DesignTypesList.DesignTypes
                .Select(o => o.DocPackage)
                .Distinct()
                .OrderBy(o => o)
                .ToList();
            DockPackages.Insert(0, _filterValueForNofiltering);
            SelectedDockPackage = DockPackages.FirstOrDefault();

            //CollectionViewSource.GetDefaultView(DesignTypes).Filter = new Predicate<object>(FilterByDocPackage);

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
            if(DesignTypesList.DesignTypes.Count == 0) {
                ErrorText = "Не удалось отобрать элементы";
                return false;
            }

            if(DesignTypesList.DesignTypes.FirstOrDefault(o => o.IsCheck) is null) {
                ErrorText = "Не выбран ни один тип конструкции";
                return false;
            }

            if(DesignTypesList.DesignTypes.FirstOrDefault(o => o.IsCheck && o.AlreadyCalculated) is null) {
                ErrorText = "Не рассчитан ни один выбранный тип конструкции";
                return false;
            }

            if(!DesignTypesList.DesignTypes.Where(o => o.IsCheck).All(o => o.AlreadyCalculated)) {
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

            foreach(DesignTypeVM designType in DesignTypesList.DesignTypes.Where(o => o.IsCheck)) {
                ids.AddRange(designType.Formworks.Select(e => e.RevitElement.Id));
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(ids);
        }


        private void ShowRebarElements() {
            List<ElementId> ids = new List<ElementId>();

            foreach(DesignTypeVM designType in DesignTypesList.DesignTypes.Where(o => o.IsCheck)) {
                ids.AddRange(designType.Rebars.Select(e => e.RevitElement.Id));
            }
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(ids);
        }

        private bool CanShowElements() {
            return DesignTypesList.DesignTypes.Any(o => o.IsCheck);
        }


        /// <summary>
        /// Получение инфорнмации об объеме опалубки, массе армирования и коэффициенте армирования 
        /// у выбранных типах конструкции
        /// </summary>
        private void GetInfo() => DesignTypesList.GetInfo(СalcСoefficientOnAverage);


        /// <summary>
        /// Запись значений коэффициенто армирования
        /// </summary>
        private void WriteRebarCoef() {
            //if(DesignTypes.FirstOrDefault(o => o.IsCheck) is null) {
            //    TaskDialog.Show("Ошибка!", "Выберите тип конструкции!");
            //    return;
            //}

            //foreach(DesignTypeInfoVM designType in DesignTypes.Where(o => o.IsCheck)) {
            //    // Если нет ошибок то выполняем запись значений коэффициента армирования в опалубочные элементы
            //    if(!designType.HasErrors) {
            //        using(Transaction transaction = _revitRepository.Document.StartTransaction("Запись коэффициентов армирования")) {
            //            foreach(Element elem in designType.Elements) {
            //                elem.SetParamValue("ФОП_ТИП_Армирование", designType.RebarCoef);
            //            }
            //            transaction.Commit();
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Ставит галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        private void SelectAllVisible() {
            //foreach(DesignTypeInfoVM item in DesignTypesList.DesignTypes.Where(FilterByDocPackage)) {
            //    item.IsCheck = true;
            //}
        }

        /// <summary>
        /// Снимает галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        private void UnselectAllVisible() {
            //foreach(DesignTypeInfoVM item in DesignTypes.Where(FilterByDocPackage)) {
            //    item.IsCheck = false;
            //}
        }

        ///// <summary>
        ///// Используется в качестве аргумента предиката для фильтрации списка по выбранному комплекту документации
        ///// </summary>
        //private bool FilterByDocPackage(object o) {
        //    if(SelectedDockPackage == _filterValueForNofiltering) {
        //        return true;
        //    }

        //    // Если в параметре есть какое то значение (не null и не пустая строка (у нас это тоже null))
        //    if(string.IsNullOrEmpty(SelectedDockPackage)) {
        //        return string.IsNullOrEmpty(((DesignTypeInfoVM) o).DocPackage);
        //    } else {
        //        return ((DesignTypeInfoVM) o).DocPackage is null ? false : ((DesignTypeInfoVM) o).DocPackage.Equals(SelectedDockPackage);
        //    }
        //}
    }
}
