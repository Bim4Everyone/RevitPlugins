using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;
using RevitReinforcementCoefficient.Views;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        /// <summary>
        /// Значение фильтра, когда не задана фильтрация ("<Не выбрано>")
        /// </summary>
        private string _filterValueForNoFiltering = "<Не выбрано>";

        private string _errorText = string.Empty;
        private DesignTypeListVM _designTypesList;

        private List<string> _dockPackages;
        private string _selectedDockPackage;
        private bool _calcСoefficientOnAverage = false;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, DesignTypeListVM designTypeListVM) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            DesignTypesList = designTypeListVM;

            LoadViewCommand = RelayCommand.Create<object>(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            SelectAllVisibleCommand = RelayCommand.Create(SelectAllVisible);
            UnselectAllVisibleCommand = RelayCommand.Create(UnselectAllVisible);

            ShowFormworkElementsCommand = RelayCommand.Create(ShowFormworkElements, CanShowElements);
            ShowRebarElementsCommand = RelayCommand.Create(ShowRebarElements, CanShowElements);

            GetInfoCommand = RelayCommand.Create(GetInfo, CanShowElements);
            UpdateFilteringCommand = RelayCommand.Create(DesignTypesList.UpdateFiltering);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand SelectAllVisibleCommand { get; }
        public ICommand UnselectAllVisibleCommand { get; }
        public ICommand ShowFormworkElementsCommand { get; }
        public ICommand ShowRebarElementsCommand { get; }
        public ICommand GetInfoCommand { get; }
        public ICommand UpdateFilteringCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

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
            set => this.RaiseAndSetIfChanged(ref _selectedDockPackage, value);
        }

        public string FilterValueForNoFiltering {
            get => _filterValueForNoFiltering;
            set => this.RaiseAndSetIfChanged(ref _filterValueForNoFiltering, value);
        }

        public bool СalcСoefficientOnAverage {
            get => _calcСoefficientOnAverage;
            set => this.RaiseAndSetIfChanged(ref _calcСoefficientOnAverage, value);
        }


        private void LoadView(object obj) {
            LoadConfig();
            // Получаем и распределяем элементы по типам конструкции
            // Если не нашли подходящих элементов на виде, то прекращаем работу и выводим сообщение об ошибке
            if(!DesignTypesList.GetElementsForAnalize()) {
                TaskDialog error = new TaskDialog("Ошибка!");
                error.MainInstruction = "На текущем виде не найдено ни одного элемента для работы!";
                error.MainContent = "Плагин работает с элементами следующих категорий: " +
                    "Стены, Перекрытия, Каркас несущий, Колонны, Несущие колонны, Фундамент несущей конструкции и Арматура.";
                error.Show();
                (obj as MainWindow).Close();
            }
            DesignTypesList.GetDesignTypes();
            DesignTypesList.SetFiltering(this);
            GetDockPackages();
        }

        /// <summary>
        /// Заполняем список комплектов документации по полученным элементам
        /// </summary>
        private void GetDockPackages() {
            DockPackages = DesignTypesList.DesignTypes
                .Select(o => o.DocPackage)
                .Distinct()
                .OrderBy(o => o)
                .ToList();
            DockPackages.Insert(0, _filterValueForNoFiltering);
            SelectedDockPackage = DockPackages.FirstOrDefault();

            // Обновляем вид принудительно, т.к. меняли коллекцию, по которой происходит фильтрация из кода
            DesignTypesList.UpdateFiltering();
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
        /// Получение информации об объеме опалубки, массе армирования и коэффициенте армирования 
        /// у выбранных типов конструкций
        /// </summary>
        private void GetInfo() => DesignTypesList.GetInfo(СalcСoefficientOnAverage);

        /// <summary>
        /// Запись значений коэффициенто армирования
        /// </summary>
        private void WriteRebarCoef() => DesignTypesList.WriteRebarCoef(_revitRepository.Document);

        /// <summary>
        /// Ставит галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        private void SelectAllVisible() => DesignTypesList.SelectAllVisible();

        /// <summary>
        /// Снимает галочки выбора у видимых с учетом фильтрации типов констуркций
        /// </summary>
        private void UnselectAllVisible() => DesignTypesList.UnselectAllVisible();
    }
}
