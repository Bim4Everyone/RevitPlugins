using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
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
        private readonly IMessageBoxService _messageBoxService;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            IRoomFinisher roomFinisher,
            IMessageBoxService messageBoxService
            ) {
            _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _roomFinisher = roomFinisher ?? throw new ArgumentNullException(nameof(roomFinisher));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

            RoomGetterModes = new ObservableCollection<RoomGetterMode>(
                Enum.GetValues(typeof(RoomGetterMode)).Cast<RoomGetterMode>());
            WallElevationModes = new ObservableCollection<WallElevationMode>(
                Enum.GetValues(typeof(WallElevationMode)).Cast<WallElevationMode>());
            WallTypes = new ObservableCollection<WallType>(_revitRepository.GetWallTypes().OrderBy(wt => wt.Name));

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadConfigCommand = RelayCommand.Create(LoadConfig);
        }

        public ICommand LoadConfigCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public IMessageBoxService MessageBoxService => _messageBoxService;

        public string ErrorText => Error;


        public ObservableCollection<RoomGetterMode> RoomGetterModes { get; }

        private RoomGetterMode _selectedRoomGetterMode;
        public RoomGetterMode SelectedRoomGetterMode {
            get => _selectedRoomGetterMode;
            set => RaiseAndSetIfChanged(ref _selectedRoomGetterMode, value);
        }


        public ObservableCollection<WallType> WallTypes { get; }

        private WallType _selectedWallType;
        public WallType SelectedWallType {
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
            }
        }

        public string Error => GetType()
            .GetProperties()
            .Select(prop => this[prop.Name])
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error)) ?? string.Empty;

        public string this[string columnName] {
            get {
                var error = string.Empty;
                switch(columnName) {
                    case nameof(SelectedWallType): {
                        if(SelectedWallType is null) {
                            error = "Задайте тип отделочных стен";
                        }
                        break;
                    }
                    case nameof(WallElevationByUser): {
                        if(SelectedWallElevationMode == WallElevationMode.ManualHeight) {
                            if(int.TryParse(WallElevationByUser, out int height)) {
                                if(height <= 0) {
                                    error = "Отметка верха должна быть больше 0";
                                } else if(height > 50000) {
                                    error = "Слишком большая отметка верха стены";
                                } else if(int.TryParse(WallBaseOffset, out int offset) && (height <= offset)) {
                                    error = "Отметка верха должна быть больше смещения";
                                }
                            } else {
                                error = "Отметка верха должна быть целым числом";
                            }
                        }
                        break;
                    }
                    case nameof(WallBaseOffset): {
                        if(int.TryParse(WallBaseOffset, out int offset)) {
                            if(offset < -5000) {
                                error = "Слишком большое смещение вниз";
                            } else if(offset > 5000) {
                                error = "Слишком большое смещение вверх";
                            }
                        } else {
                            error = "Смещение должно быть целым числом";
                        }
                        break;
                    }
                }
                return error;
            }
        }

        private void AcceptView() {
            SaveConfig();
            _roomFinisher.CreateWallsFinishing(_pluginConfig, out string error);
            if(!string.IsNullOrWhiteSpace(error)) {
                _messageBoxService.Show(error, "BIM", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }

        private bool CanAcceptView() {
            return string.IsNullOrWhiteSpace(ErrorText);
        }

        private void LoadConfig() {
            SelectedRoomGetterMode = _pluginConfig.RoomGetterMode;
            SelectedWallElevationMode = _pluginConfig.WallElevationMode;
            WallElevationByUser = _pluginConfig.WallElevation.ToString();
            WallBaseOffset = _pluginConfig.WallBaseOffset.ToString();
            SelectedWallType = _revitRepository.GetWallType(_pluginConfig.WallTypeId);

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            _pluginConfig.RoomGetterMode = SelectedRoomGetterMode;
            _pluginConfig.WallElevationMode = SelectedWallElevationMode;
            _pluginConfig.WallBaseOffset = int.TryParse(WallBaseOffset, out int offset) ? offset : 0;
            _pluginConfig.WallElevation = int.TryParse(WallElevationByUser, out int height) ? height : 0;
            _pluginConfig.WallTypeId = SelectedWallType.Id;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
