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

        #region Fields 
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        //private readonly RevitEventHandler _revitEventHandler;

        private string _errorText;
        private string _selectedElementsText;
        private string _selectedCategoriesText;

        private bool _enableFilter;

        private ElementScope _selectedElementScope;
        private ElementGroupType _selectedGroupType;
        private ICollection<ElementId> _selectedElementsIds;
        private List<Category> _selectedCategories;
        private List<Element> _needParameterElements;
        #endregion

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            SelectElementsCommand = RelayCommand.Create(SelectElements);
            SelectCategoriesCommand = RelayCommand.Create(SelectCategories);
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        #region Command 
        public ICommand SelectElementsCommand { get; }
        public ICommand SelectCategoriesCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        #endregion

        #region Properties 
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
        public MainWindow MainWindow { get; set; }
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
        #endregion

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

        private void SelectElements() {
            MainWindow.Hide();
            try {
                _selectedElementsIds = _revitRepository.SelectElementsOnView();
                SelectedElementsText = $"Выбранные элементы ({_selectedElementsIds.Count})";
            } finally {
                MainWindow.ShowDialog();
            }
        }

        private void SelectCategories() {
            var categoriesWindow = new CategoriesWindow();
            var categoriesViewWindow = new CategoriesViewModel(_revitRepository);
            categoriesWindow.DataContext = categoriesViewWindow;
            if(categoriesWindow.ShowDialog() == true) {
                SelectedCategories = categoriesViewWindow.GetSelectedCategories();
                SelectedCategoriesText = $"Выбранных категории: ({SelectedCategories.Count})";
            }
        }

        private void LoadView() {
            _selectedElementsIds = _revitRepository.GetSelectedElementsIds();

            SelectedElementsText = $"Выбранные элементы ({_selectedElementsIds.Count})";
            SelectedCategoriesText = $"Выбранные категории (0)";
        }

        private void AcceptView() {
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
            if(SelectedElementsIds.Count == 0 && SelectedCategories == null) {
                ErrorText = "Необходимо выбрать элементы";
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedCategories && SelectedElementScope == ElementScope.NotSelected) {
                ErrorText = "Необходимо выбрать область";
                return false;
            }

            ErrorText = null;
            return true;
        }

    }
}
