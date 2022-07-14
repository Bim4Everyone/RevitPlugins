using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.Interfaces;
using RevitOpeningPlacement.ViewModels.OpeningConfig.MepCategories;
using RevitOpeningPlacement.ViewModels.OpeningConfig.OffsetViewModels;
using RevitOpeningPlacement.ViewModels.OpeningConfig.SizeViewModels;
using RevitOpeningPlacement.ViewModels.Services;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;
        private string _messageText;
        private ObservableCollection<IMepCategoryViewModel> _mepCategories;
        private DispatcherTimer _timer; 

        public MainViewModel(UIApplication uiApplication, Models.Configs.OpeningConfig openingConfig) {
            _revitRepository = new RevitRepository(uiApplication);
            if(openingConfig.Categories.Any()) {
                MepCategories = new ObservableCollection<IMepCategoryViewModel>(openingConfig.Categories.Select(item => new MepCategoryViewModel(item)));
            } else {
                InitializeCategories();
            }

            InitializeTimer();

            SaveConfigCommand = new RelayCommand(SaveConfig, CanSaveConfig);
            SaveAsConfigCommand = new RelayCommand(SaveAsConfig, CanSaveConfig);
            LoadConfigCommand = new RelayCommand(LoadConfig);
        }

        public ObservableCollection<IMepCategoryViewModel> MepCategories {
            get => _mepCategories;
            set => this.RaiseAndSetIfChanged(ref _mepCategories, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string MessageText { 
            get => _messageText; 
            set => this.RaiseAndSetIfChanged(ref _messageText, value); 
        }

        public ICommand SaveConfigCommand { get; }
        public ICommand SaveAsConfigCommand { get; }
        public ICommand LoadConfigCommand { get; }


        private void InitializeCategories() {
            MepCategories = new ObservableCollection<IMepCategoryViewModel>() {
                GetPipeCategory(),
                GetRectangleDuct(),
                GetRoundDuct(),
                GetCableTray()
            };
        }

        private MepCategoryViewModel GetPipeCategory() => new MepCategoryViewModel {
            Name = "Трубы",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Диаметр"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/pipe.png"
        };

        private MepCategoryViewModel GetRectangleDuct() => new MepCategoryViewModel {
            Name = "Воздуховоды (прямоугольное сечение)",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Ширина"},
                new SizeViewModel(){Name ="Высота"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/rectangleDuct.png"
        };

        private MepCategoryViewModel GetRoundDuct() => new MepCategoryViewModel {
            Name = "Воздуховоды (круглое сечение)",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Диаметр"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/roundDuct.png"
        };

        private MepCategoryViewModel GetCableTray() => new MepCategoryViewModel {
            Name = "Лотки",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Ширина"},
                new SizeViewModel(){Name ="Высота"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/tray.png"
        };

        private void InitializeTimer() {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 3);
            _timer.Tick += (s, a) => {
                MessageText = null;
                _timer.Stop();
            };
        }
        private Models.Configs.OpeningConfig GetOpeningConfig() {
            var categories = MepCategories.Select(item => item.GetMepCategory()).ToList();
            var config = Models.Configs.OpeningConfig.GetOpeningConfig();
            config.Categories = categories;
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
            css.Save(config);
            MessageText = "Файл настроек успешно сохранен.";
            _timer.Start();
        }

        private void LoadConfig(object p) {
            var cls = new ConfigLoaderService();
            var config = cls.Load<Models.Configs.OpeningConfig>();
            if(config != null) {
                MepCategories = new ObservableCollection<IMepCategoryViewModel>(config.Categories.Select(item => new MepCategoryViewModel(item)));
            }
            MessageText = "Файл настроек успешно загружен.";
            _timer.Start();
        }

        private bool CanSaveConfig(object p) {
            ErrorText = MepCategories.FirstOrDefault(item => !string.IsNullOrEmpty(item.GetErrorText()))?.GetErrorText();
            return string.IsNullOrEmpty(ErrorText);
        }
    }
}