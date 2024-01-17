using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.ViewModels {
    internal class MainViewModel : BaseViewModel, IDataErrorInfo {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));

            RoomGetterModes = new ObservableCollection<RoomGetterMode>(
                Enum.GetValues(typeof(RoomGetterMode)).Cast<RoomGetterMode>());
            WallHeightModes = new ObservableCollection<WallHeightMode>(
                Enum.GetValues(typeof(WallHeightMode)).Cast<WallHeightMode>());
            WallTypes = new ObservableCollection<WallType>(_revitRepository.GetWallTypes().OrderBy(wt => wt.Name));

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadConfigCommand = RelayCommand.Create(LoadConfig);
        }

        public string Title => "Настройки отделки стен";


        public ICommand LoadConfigCommand { get; }

        public ICommand AcceptViewCommand { get; }

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
        public string WallHeightByUser {
            get => _wallHeightByUser;
            set {
                RaiseAndSetIfChanged(ref _wallHeightByUser, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }


        public bool IsWallHeightByUserEnabled => SelectedWallHeightMode == WallHeightMode.ManualHeight;


        public ObservableCollection<WallHeightMode> WallHeightModes { get; }

        private WallHeightMode _selectedWallHeightMode;
        public WallHeightMode SelectedWallHeightMode {
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
                    case nameof(WallHeightByUser): {
                        if(SelectedWallHeightMode == WallHeightMode.ManualHeight) {
                            if(int.TryParse(WallHeightByUser, out int height)) {
                                if(height <= 0) {
                                    error = "Высота должна быть больше 0";
                                } else if(height > 50000) {
                                    error = "Слишком большая высота стены";
                                }
                            } else {
                                error = "Высота должна быть целым числом";
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
        }

        private bool CanAcceptView() {
            return string.IsNullOrWhiteSpace(ErrorText);
        }

        private void LoadConfig() {
            SelectedRoomGetterMode = _pluginConfig.RoomGetterMode;
            SelectedWallHeightMode = _pluginConfig.WallHeightMode;
            WallHeightByUser = _pluginConfig.WallHeightByUser.ToString();
            WallBaseOffset = _pluginConfig.WallBaseOffset.ToString();

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            _pluginConfig.RoomGetterMode = SelectedRoomGetterMode;
            _pluginConfig.WallHeightMode = SelectedWallHeightMode;
            _pluginConfig.WallBaseOffset = int.TryParse(WallBaseOffset, out int offset) ? offset : 0;
            _pluginConfig.WallHeightByUser = int.TryParse(WallHeightByUser, out int height) ? height : 0;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
