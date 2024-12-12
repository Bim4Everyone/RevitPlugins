using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMirroredElements.Models;
using RevitMirroredElements.Views;

namespace RevitMirroredElements.ViewModels {
    internal class MainViewModel : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private string _selectedElementsText;
        private string _selectedCategoriesText;

        private bool _enableFilter;

        private ElementScope _selectedElementScope;
        private ElementGroupType _selectedGroupType;
        private List<FamilyInstance> _selectedElements;
        private List<Category> _selectedCategories;
        private List<FamilyInstance> _needParameterElements;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            SelectedElementScope = ElementScope.NotSelected;
            SelectedGroupType = ElementGroupType.NotSelected;

            SelectElementsCommand = RelayCommand.Create<MainWindow>(SelectElements);
            SelectCategoriesCommand = RelayCommand.Create(SelectCategories);
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand SelectElementsCommand { get; }
        public ICommand SelectCategoriesCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public List<FamilyInstance> SelectedElements {
            get => _selectedElements;
            set => this.RaiseAndSetIfChanged(ref _selectedElements, value);
        }
        public List<Category> SelectedCategories {
            get => _selectedCategories;
            set => this.RaiseAndSetIfChanged(ref _selectedCategories, value);
        }
        public List<FamilyInstance> NeedParameterElements {
            get => _needParameterElements;
            set => this.RaiseAndSetIfChanged(ref _needParameterElements, value);
        }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public string SelectedElementsText {
            get => _selectedElementsText;
            set => this.RaiseAndSetIfChanged(ref _selectedElementsText, value);
        }
        public string SelectedCategoriesText {
            get => _selectedCategoriesText;
            set => this.RaiseAndSetIfChanged(ref _selectedCategoriesText, value);
        }
        public bool EnableFilter {
            get => _enableFilter;
            set => this.RaiseAndSetIfChanged(ref _enableFilter, value);
        }
        public ElementScope SelectedElementScope {
            get => _selectedElementScope;
            set => this.RaiseAndSetIfChanged(ref _selectedElementScope, value);
        }
        public ElementGroupType SelectedGroupType {
            get => _selectedGroupType;
            set {
                if(_selectedGroupType != value) {
                    this.RaiseAndSetIfChanged(ref _selectedGroupType, value);
                    this.RaisePropertyChanged(nameof(IsCategoriesSelected));
                }
            }
        }

        public bool IsCategoriesSelected => SelectedGroupType == ElementGroupType.SelectedCategories;

        public void UpdateMirrorParameters() {

            using(Transaction transaction = _revitRepository.StartTransaction("Заполнение параметров зеркальности")) {

                using(var window = CreateProgressDialog(NeedParameterElements)) {
                    var progress = window.CreateProgress();
                    var ct = window.CreateCancellationToken();
                    int count = 1;

                    foreach(FamilyInstance element in NeedParameterElements) {
                        progress.Report(count++);
                        ct.ThrowIfCancellationRequested();

                        bool isMirrored = IsElementMirrored(element);
                        element.SetParamValue(SharedParamsConfig.Instance.ElementMirroring, isMirrored ? 1 : 0);
                    }
                }
                transaction.Commit();
            }
        }

        private IProgressDialogService CreateProgressDialog(List<FamilyInstance> elements) {
            var window = GetPlatformService<IProgressDialogService>();
            window.DisplayTitleFormat = $"Заполнение [{{0}}\\{{1}}]";
            window.MaxValue = elements.Count;
            window.StepValue = 10;
            window.Show();
            return window;
        }

        private bool IsElementMirrored(FamilyInstance familyInstance) {
            return familyInstance.Mirrored;
        }

        private void SelectElements(MainWindow mainWindow) {
            mainWindow.Hide();
            try {
                _selectedElements = _revitRepository.SelectElementsOnView();
                SelectedElementsText = $"Выбранные элементы ({_selectedElements?.Count ?? 0})";
            } finally {
                mainWindow.ShowDialog();
            }
        }

        private void SelectCategories() {
            var categoriesWindow = new CategoriesWindow();
            var categoriesViewWindow = new CategoriesViewModel(_revitRepository, SelectedCategories);
            categoriesWindow.DataContext = categoriesViewWindow;
            if(categoriesWindow.ShowDialog() == true) {
                SelectedCategories = categoriesViewWindow.GetSelectedCategories();
                SelectedCategoriesText = $"Выбранные категории ({SelectedCategories?.Count ?? 0})";
            }
        }

        private void LoadView() {
            LoadConfig();
            _revitRepository.UpdateParams();
            _selectedElements = _revitRepository.GetSelectedElements();

            SelectedElementsText = $"Выбранные элементы ({_selectedElements?.Count ?? 0})";
            SelectedCategoriesText = $"Выбранные категории ({SelectedCategories?.Count ?? 0})";
        }

        private void AcceptView() {
            SaveConfig();
            NeedParameterElements = SelectedGroupType == ElementGroupType.SelectedCategories
                ? _revitRepository.GetElementsFromCategories(SelectedCategories, SelectedElementScope)
                : _selectedElements.ToList();

            if(NeedParameterElements == null || !NeedParameterElements.Any()) {
                return;
            }

            UpdateMirrorParameters();


            if(EnableFilter) {
                _revitRepository.FilterOnTemporaryView(NeedParameterElements);
            } else {
                _revitRepository.SelectElementsOnMainView(NeedParameterElements);
            }
        }

        private bool CanAcceptView() {
            if(SelectedGroupType == ElementGroupType.NotSelected) {
                ErrorText = "Необходимо выбрать фильтр";
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedElements && SelectedElements.Count == 0) {
                ErrorText = "Необходимо выбрать элементы";
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedCategories && SelectedCategories == null) {
                ErrorText = "Необходимо выбрать категории";
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedCategories && SelectedElementScope == ElementScope.NotSelected) {
                ErrorText = "Необходимо выбрать область";
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting == null) {
                SelectedElementScope = ElementScope.NotSelected;
                SelectedGroupType = ElementGroupType.NotSelected;
                EnableFilter = false;
                SelectedCategories = new List<Category>();
                return;
            }

            SelectedElementScope = setting.ElementScope;
            SelectedGroupType = setting.ElementGroupType;
            EnableFilter = setting.EnableFilter;
            SelectedCategories = _revitRepository.GetSaveCategories(setting.SelectedCategories)?.ToList() ?? new List<Category>();
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.ElementScope = SelectedElementScope;
            setting.ElementGroupType = SelectedGroupType;
            setting.EnableFilter = EnableFilter;
            setting.SelectedCategories = SelectedCategories.Select(x => x.Id).ToList();
            _pluginConfig.SaveProjectConfig();
        }
    }
}
