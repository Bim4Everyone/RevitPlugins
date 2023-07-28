using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.TypeNamesProviders;
using RevitOpeningPlacement.ViewModels.Services;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;
        private string _messageText;
        private ObservableCollection<MepCategoryViewModel> _mepCategories;
        private DispatcherTimer _timer;
        private readonly RevitRepository _revitRepository;

        public MainViewModel(RevitRepository revitRepository, Models.Configs.OpeningConfig openingConfig) {
            _revitRepository = revitRepository;
            if(openingConfig.Categories.Any()) {
                MepCategories = new ObservableCollection<MepCategoryViewModel>(openingConfig.Categories.Select(item => new MepCategoryViewModel(_revitRepository, item)));
                if(MepCategories.All(item => !item.IsRound)) {
                    SetShape();
                }
            } else {
                InitializeCategories();
            }
            AddMissingCategories();

            InitializeTimer();

            SaveConfigCommand = new RelayCommand(SaveConfig, CanSaveConfig);
            SaveAsConfigCommand = new RelayCommand(SaveAsConfig, CanSaveConfig);
            LoadConfigCommand = new RelayCommand(LoadConfig);

            SelectedMepCategoryViewModel = MepCategories.FirstOrDefault(category => category.IsSelected) ?? MepCategories.First();
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


        private void InitializeCategories() {
            MepCategories = new ObservableCollection<MepCategoryViewModel>() {
                GetPipe(),
                GetRectangleDuct(),
                GetRoundDuct(),
                GetCableTray(),
                GetConduit()
            };
        }

        private MepCategoryViewModel GetPipe() => new MepCategoryViewModel(_revitRepository) {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.Pipe],
            MinSizes = new ObservableCollection<SizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Diameter]}
            },
            Offsets = new ObservableCollection<OffsetViewModel>() {
                new OffsetViewModel(new TypeNamesProvider(true))
            },
            ImageSource = "../Resources/pipe.png"
        };

        private MepCategoryViewModel GetRectangleDuct() => new MepCategoryViewModel(_revitRepository) {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.RectangleDuct],
            MinSizes = new ObservableCollection<SizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Width]},
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Height]}
            },
            Offsets = new ObservableCollection<OffsetViewModel>() {
                new OffsetViewModel(new TypeNamesProvider(false))
            },
            ImageSource = "../Resources/rectangleDuct.png"
        };

        private MepCategoryViewModel GetRoundDuct() => new MepCategoryViewModel(_revitRepository) {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.RoundDuct],
            MinSizes = new ObservableCollection<SizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Diameter]}
            },
            Offsets = new ObservableCollection<OffsetViewModel>() {
                new OffsetViewModel(new TypeNamesProvider(true))
            },
            ImageSource = "../Resources/roundDuct.png"
        };

        private MepCategoryViewModel GetCableTray() => new MepCategoryViewModel(_revitRepository) {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.CableTray],
            MinSizes = new ObservableCollection<SizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Width]},
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Height]}
            },
            Offsets = new ObservableCollection<OffsetViewModel>() {
                new OffsetViewModel(new TypeNamesProvider(false))
            },
            ImageSource = "../Resources/tray.png"
        };

        private MepCategoryViewModel GetConduit() => new MepCategoryViewModel(_revitRepository) {
            Name = RevitRepository.MepCategoryNames[MepCategoryEnum.Conduit],
            MinSizes = new ObservableCollection<SizeViewModel>() {
                new SizeViewModel(){ Name = RevitRepository.ParameterNames[Parameters.Diameter]}
            },
            Offsets = new ObservableCollection<OffsetViewModel>() {
                new OffsetViewModel(new TypeNamesProvider(false))
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

        private void SetShape() {
            SetRoundShape(MepCategoryEnum.Pipe);
            SetRoundShape(MepCategoryEnum.RoundDuct);
            SetRoundShape(MepCategoryEnum.Conduit);
        }

        private void SetRoundShape(MepCategoryEnum category) {
            var mep = MepCategories.FirstOrDefault(item => item.Name.Equals(RevitRepository.MepCategoryNames[category]));
            if(mep != null) {
                mep.IsRound = true;
                foreach(var offset in mep.Offsets) {
                    offset.Update(new TypeNamesProvider(mep.IsRound));
                }
            }
        }

        private MepCategoryViewModel GetMissingCategory(MepCategoryEnum missingCategory) {
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
            var config = Models.Configs.OpeningConfig.GetOpeningConfig(_revitRepository.Doc);
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
            css.Save(config, _revitRepository.Doc);
            MessageText = "Файл настроек успешно сохранен.";
            _timer.Start();
        }

        private void LoadConfig(object p) {
            var cls = new ConfigLoaderService();
            var config = cls.Load<Models.Configs.OpeningConfig>(_revitRepository.Doc);
            if(config != null) {
                MepCategories = new ObservableCollection<MepCategoryViewModel>(config.Categories.Select(item => new MepCategoryViewModel(_revitRepository, item)));
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