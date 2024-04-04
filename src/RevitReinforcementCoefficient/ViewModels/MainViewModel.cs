using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private string _saveProperty;
        private List<Element> _rebars;
        private List<Element> _allElements;
        private List<DesignTypeInfoVM> _designTypes;
        private DesignTypeInfoVM _selectedDesignType;

        private TypeAnalyzer _typeAnalyzer;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            ShowFormworkElementsCommand = RelayCommand.Create(ShowFormworkElements, CanShowElements);
            ShowRebarElementsCommand = RelayCommand.Create(ShowRebarElements, CanShowElements);

            GetInfoCommand = RelayCommand.Create(GetInfo, CanShowElements);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand ShowFormworkElementsCommand { get; }
        public ICommand ShowRebarElementsCommand { get; }

        public ICommand GetInfoCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        public List<Element> Rebars {
            get => _rebars;
            set => this.RaiseAndSetIfChanged(ref _rebars, value);
        }

        public List<Element> AllElements {
            get => _allElements;
            set => this.RaiseAndSetIfChanged(ref _allElements, value);
        }

        public List<DesignTypeInfoVM> DesignTypes {
            get => _designTypes;
            set => this.RaiseAndSetIfChanged(ref _designTypes, value);
        }
        public DesignTypeInfoVM SelectedDesignType {
            get => _selectedDesignType;
            set => this.RaiseAndSetIfChanged(ref _selectedDesignType, value);
        }

        private void LoadView() {
            LoadConfig();

            // Нужно ли иметь разные списки?
            AllElements = _revitRepository.ElementsByFilter;
            Rebars = _revitRepository.RebarsInActiveView;

            _typeAnalyzer = new TypeAnalyzer();

            DesignTypes = _typeAnalyzer.CheckNSortByDesignTypes(AllElements.Union(Rebars));
        }

        private void AcceptView() {
            SaveConfig();

            WriteRebarCoef();
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = "Введите значение сохраняемого свойства.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }



        private void ShowFormworkElements() {

            _revitRepository.ActiveUIDocument.Selection.SetElementIds(SelectedDesignType.Elements.Select(e => e.Id).ToList());
        }

        private void ShowRebarElements() {

            _revitRepository.ActiveUIDocument.Selection.SetElementIds(SelectedDesignType.Rebars.Select(e => e.Id).ToList());
        }

        private bool CanShowElements() {
            if(SelectedDesignType is null) {
                return false;
            }
            return true;
        }


        private void GetInfo() {

            if(SelectedDesignType is null) {
                TaskDialog.Show("Ошибка!", "Выберите тип конструкции!");
                return;
            }

            // Проверка элементов выбранного типа конструкции
            if(!SelectedDesignType.FormParamsChecked) {

                _typeAnalyzer.CheckParamsInFormElements(SelectedDesignType);
            }
            if(!SelectedDesignType.RebarParamsChecked) {

                _typeAnalyzer.CheckParamsInRebars(SelectedDesignType);
            }

            // Если есть ошибки либо в опалубке, либо в арматуре подсчет выполняться не будет, т.к. нужны оба
            if(!SelectedDesignType.HasErrors) {

                // Выполняем расчет объема опалубки, массы арматуры и коэффициента армирования у выбранного типа конструкции
                _typeAnalyzer.CalculateRebarCoef(SelectedDesignType);
            }
        }


        /// <summary>
        /// Запись значений коэффициенто армирования
        /// </summary>
        private void WriteRebarCoef() {

            if(SelectedDesignType is null) {
                TaskDialog.Show("Ошибка!", "Выберите тип конструкции!");
                return;
            }

            // Если нет ошибок то выполняем запись
            if(!SelectedDesignType.HasErrors) {

                using(Transaction transaction = _revitRepository.Document.StartTransaction("Запись коэффициентов армирования")) {
                    foreach(Element elem in SelectedDesignType.Elements) {

                        elem.SetParamValue("ФОП_ТИП_Армирование", SelectedDesignType.RebarCoef.ToString());
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
