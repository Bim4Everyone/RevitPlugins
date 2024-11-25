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
        private bool _isSelectedElements;
        private bool _isSelectedCategories;
        private bool _isActiveView;
        private bool _isWholeProject;

        private ElementScope _selectedElementScope;
        private ElementGroupType _selectedGroupType;
        private ICollection<ElementId> _selectedElementsIds;
        private List<Category> _selectedCategories;
        private List<Element> _needParameterElements;

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

        public ICollection<ElementId> SelectedElementsIds {
            get => _selectedElementsIds;
            set => this.RaiseAndSetIfChanged(ref _selectedElementsIds, value);
        }
        public List<Category> SelectedCategories {
            get => _selectedCategories;
            set => this.RaiseAndSetIfChanged(ref _selectedCategories, value);
        }
        public List<Element> NeedParameterElements {
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
            set => this.RaiseAndSetIfChanged(ref _selectedGroupType, value);
        }
        public bool IsSelectedElements {
            get => _isSelectedElements;
            set {
                if(value == true) {
                    SelectedGroupType = ElementGroupType.SelectedElements;
                }
                this.RaiseAndSetIfChanged(ref _isSelectedElements, value);
            }

        }
        public bool IsSelectedCategories {
            get => _isSelectedCategories;
            set {
                if(value == true) {
                    SelectedGroupType = ElementGroupType.SelectedCategories;
                }
                this.RaiseAndSetIfChanged(ref _isSelectedCategories, value);
            }
        }
        public bool IsActiveView {
            get => _isActiveView;
            set {
                if(value == true) {
                    SelectedElementScope = ElementScope.ActiveView;
                }
                this.RaiseAndSetIfChanged(ref _isActiveView, value);
            }

        }
        public bool WholeProject {
            get => _isWholeProject;
            set {
                if(value == true) {
                    SelectedElementScope = ElementScope.WholeProject;
                }
                this.RaiseAndSetIfChanged(ref _isWholeProject, value);
            }
        }

        public void UpdateMirrorParameters() {

            using(Transaction transaction = _revitRepository.StartTransaction("Заполнение параметров зеркальности")) {

                using(var window = CreateProgressDialog(NeedParameterElements)) {
                    var progress = window.CreateProgress();
                    var ct = window.CreateCancellationToken();
                    int count = 1;

                    foreach(Element element in NeedParameterElements) {
                        progress.Report(count++);
                        ct.ThrowIfCancellationRequested();

                        bool isMirrored = IsElementMirrored(element);
                        element.SetParamValue(SharedParamsConfig.Instance.ElementMirroring, isMirrored ? 1 : 0);
                    }
                }
                transaction.Commit();
            }
        }

        private IProgressDialogService CreateProgressDialog(IList<Element> elements) {
            var window = GetPlatformService<IProgressDialogService>();
            window.DisplayTitleFormat = $"Заполнение [{{0}}\\{{1}}]";
            window.MaxValue = elements.Count;
            window.StepValue = 10;
            window.Show();
            return window;
        }

        private bool IsElementMirrored(Element element) {
            return ((FamilyInstance) element).Mirrored;
        }

        private void SelectElements(MainWindow mainWindow) {
            mainWindow.Hide();
            try {
                _selectedElementsIds = _revitRepository.SelectElementsOnView();
                SelectedElementsText = $"Выбранные элементы ({_selectedElementsIds?.Count ?? 0})";
            } finally {
                mainWindow.ShowDialog();
            }
        }

        private void SelectCategories() {
            var categoriesWindow = new CategoriesWindow();
            var categoriesViewWindow = new CategoriesViewModel(_revitRepository);
            categoriesWindow.DataContext = categoriesViewWindow;
            if(categoriesWindow.ShowDialog() == true) {
                SelectedCategories = categoriesViewWindow.GetSelectedCategories();
                SelectedCategoriesText = $"Выбранные категории ({SelectedCategories?.Count ?? 0})";
            }
        }

        private void LoadView() {
            LoadConfig();
            _revitRepository.UpdateParams();
            _selectedElementsIds = _revitRepository.GetSelectedElementsIds();

            SelectedElementsText = $"Выбранные элементы ({_selectedElementsIds?.Count ?? 0})";
            SelectedCategoriesText = $"Выбранные категории ({SelectedCategories?.Count ?? 0})";
        }

        private void AcceptView() {
            SaveConfig();
            var needParametrElementsIds = SelectedGroupType == ElementGroupType.SelectedCategories
                ? _revitRepository.GetElementsIdsFromCategories(SelectedCategories, SelectedElementScope)
                : _selectedElementsIds;

            NeedParameterElements = _revitRepository.GetElements(needParametrElementsIds).ToList();

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
            if(SelectedGroupType == ElementGroupType.SelectedElements && SelectedElementsIds.Count == 0) {
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
            IsSelectedElements = setting.IsSelectedElements;
            IsSelectedCategories = setting.IsSelectedCategories;
            IsActiveView = setting.IsActiveView;
            WholeProject = setting.WholeProject;
            EnableFilter = setting.EnableFilter;
            SelectedCategories = _revitRepository.GetCategoriesByElementIds(setting.SelectedCategories).ToList();
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.IsSelectedElements = IsSelectedElements;
            setting.IsSelectedCategories = IsSelectedCategories;
            setting.IsActiveView = IsActiveView;
            setting.WholeProject = WholeProject;
            setting.EnableFilter = EnableFilter;
            setting.SelectedCategories = SelectedCategories.Select(x => x.Id).ToList();
            _pluginConfig.SaveProjectConfig();
        }
    }
}
