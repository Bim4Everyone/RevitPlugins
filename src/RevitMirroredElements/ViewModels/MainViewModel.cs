using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMirroredElements.Interfaces;
using RevitMirroredElements.Models;
using RevitMirroredElements.Views;

namespace RevitMirroredElements.ViewModels {
    internal class MainViewModel : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ICategorySelectionService _categorySelectionService;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _selectedElementsText;
        private string _activeViewText;
        private string _wholeProjectText; 
        private string _selectedCategoriesText;
        private string _selectedCategoriesNameText;
        private string _elementMirroringParamName;

        private bool _enableFilter;

        private ElementScope _selectedElementScope;
        private ElementGroupType _selectedGroupType;
        private List<FamilyInstance> _selectedElements;
        private List<Category> _selectedCategories;
        private List<FamilyInstance> _needParameterElements;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ICategorySelectionService categorySelectionService,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _categorySelectionService = categorySelectionService;
            _localizationService = localizationService;

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
            set {
                this.RaiseAndSetIfChanged(ref _selectedCategories, value);
                UpdateSelectedCategoriesText();
            }
        }
        public List<FamilyInstance> NeedParameterElements {
            get => _needParameterElements;
            set => this.RaiseAndSetIfChanged(ref _needParameterElements, value);
        }
        public string ElementMirroringParamName {
            get => _elementMirroringParamName;
            set => this.RaiseAndSetIfChanged(ref _elementMirroringParamName, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public string SelectedElementsText {
            get => _selectedElementsText;
            set => this.RaiseAndSetIfChanged(ref _selectedElementsText, value);
        }
        public string ActiveViewText {
            get => _activeViewText;
            set => this.RaiseAndSetIfChanged(ref _activeViewText, value);
        }
        public string WholeProjectText {
            get => _wholeProjectText;
            set => this.RaiseAndSetIfChanged(ref _wholeProjectText, value);
        }
        public string SelectedCategoriesText {
            get => _selectedCategoriesText;
            set => this.RaiseAndSetIfChanged(ref _selectedCategoriesText, value);
        }
        public string SelectedCategoriesNameText {
            get => _selectedCategoriesNameText;
            set => this.RaiseAndSetIfChanged(ref _selectedCategoriesNameText, value);
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
            var transactionText = _localizationService.GetLocalizedString("Transaction.SetParam");
            using(Transaction transaction = _revitRepository.Document.StartTransaction(transactionText)) {

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
            var progressTitleText = _localizationService.GetLocalizedString("MainWindow.ProgressTitleFormat");
            window.DisplayTitleFormat = $"{progressTitleText} [{{0}}\\{{1}}]";
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
                SelectedElementsText = _localizationService.GetLocalizedString(
                     "MainWindow.SelectedElements",
                     _selectedElements?.Count ?? 0
                );
            } finally {
                mainWindow.ShowDialog();
            }
        }

        private void SelectCategories() {
            var previousCategories = SelectedCategories?.ToList() ?? new List<Category>();

            var result = _categorySelectionService.SelectCategories(previousCategories);

            if(result == null) {
                SelectedCategories = previousCategories;
            } else {
                SelectedCategories = result;
            }

            SelectedCategoriesText = _localizationService.GetLocalizedString(
                "MainWindow.SelectedCategories",
                SelectedCategories?.Count ?? 0
            );
        }

        private void UpdateSelectedCategoriesText() {
            var selectedCategoriesText = _localizationService.GetLocalizedString("MainWindow.CheckMirrorCategories");
            if(SelectedCategories == null || SelectedCategories.Count == 0) {
                SelectedCategoriesNameText = $"{selectedCategoriesText}:\n ";
                return;
            }

            var categoryNames = SelectedCategories
               .Select(c => c.Name)
               .Take(5)
               .ToList();

            string preview = string.Join(", ", categoryNames);

            if(SelectedCategories.Count > 5) {
                preview += ", ...";
            }

            SelectedCategoriesNameText = $"{selectedCategoriesText}:\n• {preview}";
        }

        private void LoadView() {
            LoadConfig();
            _revitRepository.UpdateParams();
            _selectedElements = _revitRepository.GetSelectedElements();
            ElementMirroringParamName = SharedParamsConfig.Instance.ElementMirroring.Name;

            var activeViewText = _localizationService.GetLocalizedString("MainWindow.ActiveView");
            var wholeProjectText = _localizationService.GetLocalizedString("MainWindow.WholeProject");

            ActiveViewText = _localizationService.GetLocalizedString(
                "MainWindow.CheckMirrorCategoriesElements",
                activeViewText.ToLower()
            );

            WholeProjectText = _localizationService.GetLocalizedString(
                "MainWindow.CheckMirrorCategoriesElements",
                wholeProjectText.ToLower()
            );

            SelectedElementsText = _localizationService.GetLocalizedString(
                "MainWindow.SelectedElements",
                _selectedElements?.Count ?? 0
            );

            SelectedCategoriesText = _localizationService.GetLocalizedString(
                "MainWindow.SelectedCategories",
                SelectedCategories?.Count ?? 0
            );
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
                var transactionName = _localizationService.GetLocalizedString("Transaction.SettingTemporaryView");
                using(Transaction transaction = _revitRepository.Document.StartTransaction(transactionName)) {
                    _revitRepository.FilterOnTemporaryView(NeedParameterElements);
                    transaction.Commit();
                } 
            } else {
                _revitRepository.SelectElementsOnMainView(NeedParameterElements);
            }
        }

        private bool CanAcceptView() {
            if(SelectedGroupType == ElementGroupType.NotSelected) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectFilter");
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedElements && SelectedElements.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectElements");
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedCategories && (SelectedCategories == null || SelectedCategories.Count == 0)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectCategories");
                return false;
            }
            if(SelectedGroupType == ElementGroupType.SelectedCategories && SelectedElementScope == ElementScope.NotSelected) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectScope");
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SelectedElementScope = setting?.ElementScope ?? ElementScope.NotSelected;
            SelectedGroupType = setting?.ElementGroupType ?? ElementGroupType.NotSelected;
            EnableFilter = setting?.EnableFilter ?? false;

            SelectedCategories = _revitRepository.GetSaveCategories(setting?.SelectedCategories).ToList();
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
