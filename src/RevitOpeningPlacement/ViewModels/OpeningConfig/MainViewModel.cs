using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.ViewModels.Services;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;
        private string _messageText;
        private ObservableCollection<MepCategoryViewModel> _mepCategories;
        private DispatcherTimer _timer;
        private readonly RevitRepository _revitRepository;

        public MainViewModel(RevitRepository revitRepository, Models.Configs.OpeningConfig openingConfig) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(openingConfig is null) {
                throw new ArgumentNullException(nameof(openingConfig));
            }
            if(openingConfig.Categories.Count != new MepCategoryCollection().Count) {
                throw new ArgumentException("Файл конфигурации некорректен, нужно удалить его.", nameof(openingConfig));
            }
            MepCategories = new ObservableCollection<MepCategoryViewModel>(
                openingConfig.Categories.Select(item => new MepCategoryViewModel(_revitRepository, item)));

            InitializeTimer();

            SaveConfigCommand = new RelayCommand(SaveConfig, CanSaveConfig);
            SaveAsConfigCommand = new RelayCommand(SaveAsConfig, CanSaveConfig);
            LoadConfigCommand = new RelayCommand(LoadConfig);
            CheckSearchSearchSetCommand = new RelayCommand(CheckSearchSet, CanSaveConfig);

            SelectedMepCategoryViewModel = MepCategories.FirstOrDefault(category => category.IsSelected)
                ?? MepCategories.First();
            foreach(MepCategoryViewModel mepCategoryViewModel in MepCategories) {
                mepCategoryViewModel.PropertyChanged += MepCategoryIsSelectedPropertyChanged;
            }
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
        public ICommand CheckSearchSearchSetCommand { get; }


        private void InitializeTimer() {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 3);
            _timer.Tick += (s, a) => {
                MessageText = null;
                _timer.Stop();
            };
        }

        private void CheckSearchSet(object p) {
            SaveConfig(null);

            var categories = new MepCategoryCollection(MepCategories.Select(item => item.GetMepCategory()));
            var configurator = new PlacementConfigurator(_revitRepository, categories);

            MepCategory mepCategory = SelectedMepCategoryViewModel.GetMepCategory();
            Filter linearFilter = configurator.GetLinearFilter(mepCategory);
            Filter nonLinearFilter = configurator.GetFittingFilter(mepCategory);

            var vm = new MepCategoryFilterViewModel(_revitRepository, linearFilter, nonLinearFilter);
            var view = new MepCategoryFilterView() { DataContext = vm };
            view.Show();
        }

        private Models.Configs.OpeningConfig GetOpeningConfig() {
            var config = Models.Configs.OpeningConfig.GetOpeningConfig(_revitRepository.Doc);
            config.Categories = new MepCategoryCollection(MepCategories.Select(item => item.GetMepCategory()));
            config.ShowPlacingErrors = ShowPlacingErrors;
            return config;
        }

        private void SaveConfig(object p) {
            GetOpeningConfig().SaveProjectConfig();
            MessageText = "Файл настроек успешно сохранен.";
            _timer.Start();
        }

        private void SaveAsConfig(object p) {
            var config = GetOpeningConfig();
            var css = new ConfigSaverService();
            css.Save(config, _revitRepository.Doc);
            MessageText = "Файл настроек успешно сохранен.";
            _timer.Start();
        }

        private void LoadConfig(object p) {
            var cls = new ConfigLoaderService();
            var config = cls.Load<Models.Configs.OpeningConfig>(_revitRepository.Doc);
            if(config != null) {
                ShowPlacingErrors = config.ShowPlacingErrors;
                MepCategories = new ObservableCollection<MepCategoryViewModel>(
                    config.Categories.Select(item => new MepCategoryViewModel(_revitRepository, item)));
                SelectedMepCategoryViewModel = MepCategories.FirstOrDefault(category => category.IsSelected)
                    ?? MepCategories.First();
            }
            MessageText = "Файл настроек успешно загружен.";
            _timer.Start();
        }

        private bool CanSaveConfig(object p) {
            ErrorText = MepCategories.FirstOrDefault(item => !string.IsNullOrEmpty(item.GetErrorText()))
                ?.GetErrorText();
            return string.IsNullOrEmpty(ErrorText);
        }

        /// <summary>
        /// Если пользователь включил CheckBox у категории инженерных элементов, то сделать ее выбранной
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
    }
}
