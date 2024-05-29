using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Models.Enums;
using RevitFinishingWalls.Services.Creation;

namespace RevitFinishingWalls.ViewModels {
    internal class MainViewModel : BaseViewModel, IDataErrorInfo {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly IRoomFinisher _roomFinisher;

        /// <summary>Максимальная допустимая отметка верха отделочной стены в мм</summary>
        private const int _wallTopMaxElevationMM = 50000;
        /// <summary>Максимальное смещение низа стены вверх в мм</summary>
        private const int _wallBaseMaxOffsetMM = 5000;
        /// <summary>Минимальное смещение низа стены вниз в мм</summary>
        private const int _wallBaseMinOffsetMM = -5000;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            IRoomFinisher roomFinisher
            ) {
            _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _roomFinisher = roomFinisher ?? throw new ArgumentNullException(nameof(roomFinisher));

            RoomGetterModes = new ObservableCollection<RoomGetterMode>(
                Enum.GetValues(typeof(RoomGetterMode)).Cast<RoomGetterMode>());
            WallElevationModes = new ObservableCollection<WallElevationMode>(
                Enum.GetValues(typeof(WallElevationMode)).Cast<WallElevationMode>());
            WallTypes = new ObservableCollection<WallTypeViewModel>(
                _revitRepository.GetWallTypes().Select(wt => new WallTypeViewModel(wt)).OrderBy(wt => wt.Name));

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadConfigCommand = RelayCommand.Create(LoadConfig);
        }

        public ICommand LoadConfigCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText => Error;


        public ObservableCollection<RoomGetterMode> RoomGetterModes { get; }

        private RoomGetterMode _selectedRoomGetterMode;
        public RoomGetterMode SelectedRoomGetterMode {
            get => _selectedRoomGetterMode;
            set => RaiseAndSetIfChanged(ref _selectedRoomGetterMode, value);
        }


        public ObservableCollection<WallTypeViewModel> WallTypes { get; }

        private WallTypeViewModel _selectedWallType;
        public WallTypeViewModel SelectedWallType {
            get => _selectedWallType;
            set {
                RaiseAndSetIfChanged(ref _selectedWallType, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }


        private string _wallHeightByUser;
        public string WallElevationByUser {
            get => _wallHeightByUser;
            set {
                RaiseAndSetIfChanged(ref _wallHeightByUser, value);
                OnPropertyChanged(nameof(ErrorText));
                OnPropertyChanged(nameof(WallBaseOffset));
            }
        }


        public bool IsWallHeightByUserEnabled => SelectedWallElevationMode == WallElevationMode.ManualHeight;


        public ObservableCollection<WallElevationMode> WallElevationModes { get; }

        private WallElevationMode _selectedWallHeightMode;
        public WallElevationMode SelectedWallElevationMode {
            get => _selectedWallHeightMode;
            set {
                RaiseAndSetIfChanged(ref _selectedWallHeightMode, value);
                OnPropertyChanged(nameof(IsWallHeightByUserEnabled));
                OnPropertyChanged(nameof(ErrorText));
            }
        }


        private string _wallBaseOffset;
        public string WallBaseOffset {
            get => _wallBaseOffset;
            set {
                RaiseAndSetIfChanged(ref _wallBaseOffset, value);
                OnPropertyChanged(nameof(ErrorText));
                OnPropertyChanged(nameof(WallElevationByUser));
            }
        }

        public string Error => GetType()
            .GetProperties()
            .Select(prop => this[prop.Name])
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error)) ?? string.Empty;

        public string this[string columnName] {
            get {
                switch(columnName) {
                    case nameof(SelectedWallType): {
                        if(SelectedWallType is null) {
                            return "Задайте тип отделочных стен";
                        }
                        break;
                    }
                    case nameof(WallElevationByUser): {
                        if(SelectedWallElevationMode == WallElevationMode.ManualHeight) {
                            if(double.TryParse(WallElevationByUser, out double height)) {
                                if(height <= 0) {
                                    return "Отметка верха должна быть больше 0";
                                } else if(height > _wallTopMaxElevationMM) {
                                    return "Слишком большая отметка верха стены";
                                } else if(double.TryParse(WallBaseOffset, out double offset) && (height <= offset)) {
                                    return "Отметка верха должна быть больше смещения";
                                }
                            } else {
                                return "Отметка верха должна быть числом";
                            }
                        }
                        break;
                    }
                    case nameof(WallBaseOffset): {
                        if(double.TryParse(WallBaseOffset, out double offset)) {
                            if(offset < _wallBaseMinOffsetMM) {
                                return "Слишком большое смещение вниз";
                            } else if(offset > _wallBaseMaxOffsetMM) {
                                return "Слишком большое смещение вверх";
                            }
                        } else {
                            return "Смещение должно быть числом";
                        }
                        break;
                    }
                }
                return string.Empty;
            }
        }

        private void AcceptView() {
            SaveConfig();
            _roomFinisher.CreateWallsFinishing(_pluginConfig, out string error);
            //if(!string.IsNullOrWhiteSpace(error)) {
            //    var errorMsgService = ServicesProvider.GetPlatformService<RichErrorMessageService>();
            //    errorMsgService.ShowErrorMessage(error);
            //}
        }

        private bool CanAcceptView() {
            return string.IsNullOrWhiteSpace(ErrorText);
        }

        private void LoadConfig() {
            SelectedRoomGetterMode = _pluginConfig.RoomGetterMode;
            SelectedWallElevationMode = _pluginConfig.WallElevationMode;
            WallElevationByUser = _pluginConfig.WallElevationMm.ToString();
            WallBaseOffset = _pluginConfig.WallBaseOffsetMm.ToString();
            SelectedWallType = WallTypes.FirstOrDefault(wtvm => wtvm.WallTypeId == _pluginConfig.WallTypeId);

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            _pluginConfig.RoomGetterMode = SelectedRoomGetterMode;
            _pluginConfig.WallElevationMode = SelectedWallElevationMode;
            _pluginConfig.WallBaseOffsetMm = double.TryParse(WallBaseOffset, out double offset) ? offset : 0;
            _pluginConfig.WallElevationMm = double.TryParse(WallElevationByUser, out double height) ? height : 0;
            _pluginConfig.WallTypeId = SelectedWallType.WallTypeId;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
