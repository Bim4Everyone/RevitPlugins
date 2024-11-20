using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Services;
using RevitOpeningPlacement.ViewModels.Services;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;
        private string _messageText;
        private ObservableCollection<MepCategoryViewModel> _mepCategories;
        private DispatcherTimer _timer;
        private readonly RevitRepository _revitRepository;
        private readonly ConfigFileService _configFileService;
        private string _configName;

        public MainViewModel(
            RevitRepository revitRepository,
            ConfigFileService configFileService,
            Models.Configs.OpeningConfig openingConfig) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _configFileService = configFileService ?? throw new ArgumentNullException(nameof(configFileService));

            if(openingConfig is null) {
                throw new ArgumentNullException(nameof(openingConfig));
            }
            MepCategories = new ObservableCollection<MepCategoryViewModel>(
                openingConfig.Categories.Select(item => new MepCategoryViewModel(_revitRepository, item)));
            ConfigName = openingConfig.Name;

            InitializeTimer();

            SaveConfigCommand = RelayCommand.Create(SaveConfig, CanSaveConfig);
            SaveAsConfigCommand = RelayCommand.Create(SaveAsConfig, CanSaveConfig);
            LoadConfigCommand = RelayCommand.Create(LoadConfig);
            OpenConfigFolderCommand = RelayCommand.Create(OpenConfigFolder);
            CheckMepFilterCommand = RelayCommand.Create(CheckMepFilter, CanSaveConfig);
            CheckWallFilterCommand = RelayCommand.Create(CheckWallFilter, CanSaveConfig);
            CheckFloorFilterCommand = RelayCommand.Create(CheckFloorFilter, CanSaveConfig);

            SelectedMepCategoryViewModel = MepCategories.FirstOrDefault(category => category.IsSelected)
                ?? MepCategories.First();
            foreach(MepCategoryViewModel mepCategoryViewModel in MepCategories) {
                mepCategoryViewModel.PropertyChanged += MepCategoryIsSelectedPropertyChanged;
            }
        }


        public string ConfigName {
            get => _configName;
            set => RaiseAndSetIfChanged(ref _configName, value);
        }

        private MepCategoryViewModel _selectedMepCategoryViewModel;
        public MepCategoryViewModel SelectedMepCategoryViewModel {
            get => _selectedMepCategoryViewModel;
            set => RaiseAndSetIfChanged(ref _selectedMepCategoryViewModel, value);
        }

        public ObservableCollection<MepCategoryViewModel> MepCategories {
            get => _mepCategories;
            set => RaiseAndSetIfChanged(ref _mepCategories, value);
        }

        private bool _showPlacingErrors;
        public bool ShowPlacingErrors {
            get => _showPlacingErrors;
            set => RaiseAndSetIfChanged(ref _showPlacingErrors, value);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string MessageText {
            get => _messageText;
            set => RaiseAndSetIfChanged(ref _messageText, value);
        }

        public ICommand SaveConfigCommand { get; }
        public ICommand SaveAsConfigCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand CheckMepFilterCommand { get; }
        public ICommand CheckWallFilterCommand { get; }
        public ICommand CheckFloorFilterCommand { get; }

        public ICommand OpenConfigFolderCommand { get; }


        private void InitializeTimer() {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 3);
            _timer.Tick += (s, a) => {
                MessageText = null;
                _timer.Stop();
            };
        }

        private void CheckMepFilter() {
            SaveConfig();

            var categories = new MepCategoryCollection(MepCategories.Select(item => item.GetMepCategory()));
            var configurator = new PlacementConfigurator(_revitRepository, categories);

            MepCategory mepCategory = SelectedMepCategoryViewModel.GetMepCategory();
            Filter linearFilter = configurator.GetLinearFilter(mepCategory);
            Filter nonLinearFilter = configurator.GetFittingFilter(mepCategory);

            var vm = new MepCategoryFilterViewModel(_revitRepository, linearFilter, nonLinearFilter);
            var view = new MepCategoryFilterView() { DataContext = vm };
            view.Show();
        }

        private void CheckWallFilter() {
            SaveConfig();
            ShowStructureFilter(StructureCategoryEnum.Wall);
        }

        private void CheckFloorFilter() {
            SaveConfig();
            ShowStructureFilter(StructureCategoryEnum.Floor);
        }

        private void ShowStructureFilter(StructureCategoryEnum structureCategory) {
            Filter filter = GetCurrentStructureFilter(structureCategory);
            var vm = new StructureCategoryFilterViewModel(_revitRepository, filter);
            var view = new StructureCategoryFilterView() { DataContext = vm };
            view.Show();
        }

        private Filter GetCurrentStructureFilter(StructureCategoryEnum structureCategory) {
            MepCategory mepCategory = SelectedMepCategoryViewModel.GetMepCategory();
            string structureName = RevitRepository.StructureCategoryNames[structureCategory];
            return new Filter(_revitRepository.GetClashRevitRepository()) {
                CategoryIds = new List<ElementId>(
                    _revitRepository.GetCategories(structureCategory)
                    .Select(c => c.Id)),
                Name = structureName,
                Set = mepCategory.Intersections
                    .First(c => c.Name.Equals(structureName, StringComparison.CurrentCultureIgnoreCase))
                    .Set
            };
        }

        private Models.Configs.OpeningConfig GetOpeningConfig() {
            var config = Models.Configs.OpeningConfig.GetOpeningConfig(_revitRepository.Doc);
            config.Categories = new MepCategoryCollection(MepCategories.Select(item => item.GetMepCategory()));
            config.ShowPlacingErrors = ShowPlacingErrors;
            config.Name = ConfigName.Trim();
            return config;
        }

        private void SaveConfig() {
            var config = GetOpeningConfig();
            config.SaveProjectConfig();
            UpdateOpeningConfigPath(config.ProjectConfigPath);
            MessageText = "Файл настроек успешно сохранен.";
            _timer.Start();
        }

        private void SaveAsConfig() {
            var config = GetOpeningConfig();
            var css = new ConfigSaverService();
            var path = css.Save(config, _revitRepository.Doc);
            UpdateOpeningConfigPath(path);
            MessageText = "Файл настроек успешно сохранен.";
            _timer.Start();
        }

        private void LoadConfig() {
            var cls = new ConfigLoaderService();
            var config = cls.Load<Models.Configs.OpeningConfig>(_revitRepository.Doc);
            if(config != null) {
                ShowPlacingErrors = config.ShowPlacingErrors;
                MepCategories = new ObservableCollection<MepCategoryViewModel>(
                    config.Categories.Select(item => new MepCategoryViewModel(_revitRepository, item)));
                SelectedMepCategoryViewModel = MepCategories.FirstOrDefault(category => category.IsSelected)
                    ?? MepCategories.First();
                ConfigName = config.Name;
                UpdateOpeningConfigPath(config.ProjectConfigPath);
            }
            MessageText = "Файл настроек успешно загружен.";
            _timer.Start();
        }

        private void UpdateOpeningConfigPath(string path) {
            var mepConfigPath = MepConfigPath.GetMepConfigPath(_revitRepository.Doc);
            mepConfigPath.OpeningConfigPath = path;
            mepConfigPath.SaveProjectConfig();
        }

        private bool CanSaveConfig() {
            if(string.IsNullOrWhiteSpace(ConfigName)) {
                ErrorText = "Укажите название настроек.";
                return false;
            }
            if(ConfigName.Length > 100) {
                ErrorText = "Слишком длинное название настроек.";
                return false;
            }
            ErrorText = MepCategories.FirstOrDefault(item => !string.IsNullOrEmpty(item.GetErrorText()))
                ?.GetErrorText();
            return string.IsNullOrEmpty(ErrorText);
        }

        /// <summary>
        /// Если пользователь включил CheckBox у категории инженерных элементов, то сделать ее выбранной
        /// </summary>
        private void MepCategoryIsSelectedPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if((e != null)
                && string.Equals(e.PropertyName, nameof(MepCategoryViewModel.IsSelected))
                && (sender != null)
                && (sender is MepCategoryViewModel mepCategoryViewModel)) {

                if(mepCategoryViewModel.IsSelected) {
                    SelectedMepCategoryViewModel = mepCategoryViewModel;
                }
            }
        }

        private void OpenConfigFolder() {
            var path = GetOpeningConfig().ProjectConfigPath;
            var dir = Path.GetDirectoryName(path);
            _configFileService.OpenFolder(dir, path);
        }
    }
}
