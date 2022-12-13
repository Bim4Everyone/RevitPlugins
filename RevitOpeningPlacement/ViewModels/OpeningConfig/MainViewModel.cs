using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;
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
        private string _errorText;
        private string _messageText;
        private ObservableCollection<IMepCategoryViewModel> _mepCategories;
        private DispatcherTimer _timer;

        public MainViewModel(UIApplication uiApplication, Models.Configs.OpeningConfig openingConfig) {
            if(openingConfig.Categories.Any()) {
                MepCategories = new ObservableCollection<IMepCategoryViewModel>(openingConfig.Categories.Select(item => new MepCategoryViewModel(item)));
            } else {
                InitializeCategories();
            }
            AddMissingCategories();

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
                GetPipe(),
                GetRectangleDuct(),
                GetRoundDuct(),
                GetCableTray(),
                GetConduit()
            };
        }

        private MepCategoryViewModel GetPipe() => new MepCategoryViewModel {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.Pipe],
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Diameter]}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/pipe.png"
        };

        private MepCategoryViewModel GetRectangleDuct() => new MepCategoryViewModel {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.RectangleDuct],
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Width]},
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Height]}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/rectangleDuct.png"
        };

        private MepCategoryViewModel GetRoundDuct() => new MepCategoryViewModel {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.RoundDuct],
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Diameter]}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/roundDuct.png"
        };

        private MepCategoryViewModel GetCableTray() => new MepCategoryViewModel {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.CableTray],
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Width]},
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Height]}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/tray.png"
        };

        private MepCategoryViewModel GetConduit() => new MepCategoryViewModel {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.Conduit],
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Diameter]}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            },
            ImageSource = "../Resources/conduit.png"
        };

        private void AddMissingCategories() {
            if(RevitRepository.MepCategoryNames.Count <= MepCategories.Count) {
                return;
            }
            foreach(var mepCategoryName in RevitRepository.MepCategoryNames) {
                if(!MepCategories.Any(item => item.Name.Equals(mepCategoryName.Value, StringComparison.CurrentCulture))) {
                    MepCategories.Add(GetMissingCategory(mepCategoryName.Key));
                }
            }
        }

        private IMepCategoryViewModel GetMissingCategory(MepCategoryEnum missingCategory) {
            switch(missingCategory) {
                case MepCategoryEnum.Pipe:
                return GetPipe();
                case MepCategoryEnum.RectangleDuct:
                return GetRectangleDuct();
                case MepCategoryEnum.RoundDuct:
                return GetRoundDuct();
                case MepCategoryEnum.CableTray:
                return GetCableTray();
                case MepCategoryEnum.Conduit:
                return GetConduit();
                default:
                throw new ArgumentException($"Не найдена категория \"{missingCategory}\".", nameof(missingCategory));
            }
        }
        private void InitializeTimer() {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 3);
            _timer.Tick += (s, a) => {
                MessageText = null;
                _timer.Stop();
            };
        }
        private Models.Configs.OpeningConfig GetOpeningConfig() {
            var config = Models.Configs.OpeningConfig.GetOpeningConfig();
            config.Categories = new MepCategoryCollection(MepCategories.Select(item => item.GetMepCategory()));
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