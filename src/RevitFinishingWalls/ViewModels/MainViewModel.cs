using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Models.Enums;
using RevitFinishingWalls.Services;
using RevitFinishingWalls.Services.Creation;

namespace RevitFinishingWalls.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly RichErrorMessageService _errorMessageService;
        private readonly IRoomFinisher _roomFinisher;
        private readonly IWallCreatorFactory _wallCreatorFactory;
        private readonly IProgressDialogFactory _progressDialogFactory;
        private readonly ILocalizationService _localizationService;

        /// <summary>Максимальная допустимая отметка верха отделочной стены в мм</summary>
        private const int _wallTopMaxElevationMM = 50000;
        /// <summary>Максимальное смещение низа стены вверх в мм</summary>
        private const int _wallBaseMaxOffsetMM = 5000;
        /// <summary>Минимальное смещение низа стены вниз в мм</summary>
        private const int _wallBaseMinOffsetMM = -5000;
        /// <summary>Максимальное смещение стены внутрь помещения в мм</summary>
        private const int _wallSideMaxOffsetMM = 200;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            RichErrorMessageService errorMessageService,
            IRoomFinisher roomFinisher,
            IWallCreatorFactory wallCreatorFactory,
            IProgressDialogFactory progressDialogFactory,
            ILocalizationService localizationService
            ) {
            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _errorMessageService = errorMessageService
                ?? throw new ArgumentNullException(nameof(errorMessageService));
            _roomFinisher = roomFinisher
                ?? throw new ArgumentNullException(nameof(roomFinisher));
            _wallCreatorFactory = wallCreatorFactory
                ?? throw new ArgumentNullException(nameof(wallCreatorFactory));
            _progressDialogFactory = progressDialogFactory
                ?? throw new ArgumentNullException(nameof(progressDialogFactory));
            _localizationService = localizationService
                ?? throw new ArgumentNullException(nameof(localizationService));
            RoomGetterModes = [.. Enum.GetValues(typeof(RoomGetterMode))
                .Cast<RoomGetterMode>()
                .Select(r => new RoomGetterModeViewModel(_localizationService, r))];
            WallElevationModes = [.. Enum.GetValues(typeof(WallElevationMode))
                .Cast<WallElevationMode>()
                .Select(w => new WallElevationModeViewModel(_localizationService, w))];
            WallTypes = [.. _revitRepository.GetWallTypes()
                .Select(wt => new WallTypeViewModel(wt))
                .OrderBy(wt => wt.Name)];
            WallHeightStyles = [.. Enum.GetValues(typeof(WallHeightStyle))
                .Cast<WallHeightStyle>()
                .Select(w => new WallHeightStyleViewModel(_localizationService, w))];

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadConfigCommand = RelayCommand.Create(LoadConfig);
        }

        public ICommand LoadConfigCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public IProgressDialogFactory ProgressDialogFactory => _progressDialogFactory;


        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }


        public ObservableCollection<RoomGetterModeViewModel> RoomGetterModes { get; }

        private RoomGetterModeViewModel _selectedRoomGetterMode;
        public RoomGetterModeViewModel SelectedRoomGetterMode {
            get => _selectedRoomGetterMode;
            set => RaiseAndSetIfChanged(ref _selectedRoomGetterMode, value);
        }


        public ObservableCollection<WallTypeViewModel> WallTypes { get; }

        private WallTypeViewModel _selectedWallType;
        public WallTypeViewModel SelectedWallType {
            get => _selectedWallType;
            set => RaiseAndSetIfChanged(ref _selectedWallType, value);
        }


        private string _wallHeightByUser;
        public string WallElevationByUser {
            get => _wallHeightByUser;
            set => RaiseAndSetIfChanged(ref _wallHeightByUser, value);
        }


        public bool IsWallHeightByUserEnabled =>
            SelectedWallTopElevationMode?.ElevationMode == WallElevationMode.ManualHeight;


        public ObservableCollection<WallElevationModeViewModel> WallElevationModes { get; }

        private WallElevationModeViewModel _selectedWallTopHeightMode;
        public WallElevationModeViewModel SelectedWallTopElevationMode {
            get => _selectedWallTopHeightMode;
            set {
                RaiseAndSetIfChanged(ref _selectedWallTopHeightMode, value);
                OnPropertyChanged(nameof(IsWallHeightByUserEnabled));
            }
        }

        public bool IsWallBaseOffsetByUserEnabled =>
            SelectedWallBaseElevationMode?.ElevationMode == WallElevationMode.ManualHeight;


        private WallElevationModeViewModel _selectedWallBaseHeightMode;
        public WallElevationModeViewModel SelectedWallBaseElevationMode {
            get => _selectedWallBaseHeightMode;
            set {
                RaiseAndSetIfChanged(ref _selectedWallBaseHeightMode, value);
                OnPropertyChanged(nameof(IsWallBaseOffsetByUserEnabled));
            }
        }

        public ObservableCollection<WallHeightStyleViewModel> WallHeightStyles { get; }

        private WallHeightStyleViewModel _selectedWallHeightStyle;
        public WallHeightStyleViewModel SelectedWallHeightStyle {
            get => _selectedWallHeightStyle;
            set => RaiseAndSetIfChanged(ref _selectedWallHeightStyle, value);
        }


        private string _wallBaseOffset;
        public string WallBaseOffset {
            get => _wallBaseOffset;
            set => RaiseAndSetIfChanged(ref _wallBaseOffset, value);
        }

        private string _wallSideOffset;
        public string WallSideOffset {
            get => _wallSideOffset;
            set => RaiseAndSetIfChanged(ref _wallSideOffset, value);
        }

        private void AcceptView() {
            SaveConfig();
            ICollection<RoomErrorsViewModel> errors;
            RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document);
            using(var progressDialogService = _progressDialogFactory.CreateDialog()) {
                var rooms = _revitRepository.GetRooms(settings.RoomGetterMode);
                progressDialogService.StepValue = 5;
                progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("ProgressBar.Title");
                var progress = progressDialogService.CreateProgress();
                progressDialogService.MaxValue = rooms.Count;
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                errors = _roomFinisher.CreateWallsFinishing(
                    rooms,
                    settings,
                    _wallCreatorFactory.Create(settings),
                    progress,
                    ct);
            }
            if(errors.Count > 0) {
                _errorMessageService.ShowErrorWindow(errors);
            }
        }

        private bool CanAcceptView() {
            if(SelectedWallType is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.WallType");
                return false;
            }
            if(SelectedRoomGetterMode is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.RoomGetterMode");
                return false;
            }
            if(SelectedWallTopElevationMode is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.WallTopElevationMode");
                return false;
            }
            if(SelectedWallBaseElevationMode is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.WallBaseElevationMode");
                return false;
            }
            if(SelectedWallBaseElevationMode?.ElevationMode == WallElevationMode.ManualHeight) {
                if(double.TryParse(WallBaseOffset, out double baseOffset)) {
                    if(baseOffset < _wallBaseMinOffsetMM) {
                        ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.MinBaseOffset");
                        return false;
                    } else if(baseOffset > _wallBaseMaxOffsetMM) {
                        ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.MaxBaseOffset");
                        return false;
                    }
                } else {
                    ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.BaseOffsetNotNumber");
                    return false;
                }
            }
            if(SelectedWallTopElevationMode?.ElevationMode == WallElevationMode.ManualHeight) {
                if(double.TryParse(WallElevationByUser, out double height)) {
                    if(height <= 0) {
                        ErrorText = string.Format(
                            _localizationService.GetLocalizedString("MainWindow.Validation.MinWallElevation"), 0);
                        return false;
                    } else if(height > _wallTopMaxElevationMM) {
                        ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.MaxWallElevation");
                        return false;
                    } else if(IsWallBaseOffsetByUserEnabled
                        && double.TryParse(WallBaseOffset, out double baseOffset)
                        && height <= baseOffset) {
                        ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.ElevationBelowOffset");
                        return false;
                    }
                } else {
                    ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.ElevationNotNumber");
                    return false;
                }
            }
            if(double.TryParse(WallSideOffset, out double sideOffset)) {
                if(sideOffset < 0) {
                    ErrorText = string.Format(
                        _localizationService.GetLocalizedString("MainWindow.Validation.MinSideOffset"), 0);
                    return false;
                } else if(sideOffset > _wallSideMaxOffsetMM) {
                    ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.MaxSideOffset");
                    return false;
                }
            } else {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.Validation.SideOffsetNotNumber");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);
            SelectedRoomGetterMode = new RoomGetterModeViewModel(
                _localizationService, settings.RoomGetterMode);
            SelectedWallTopElevationMode = new WallElevationModeViewModel(
                _localizationService, settings.WallTopElevationMode);
            SelectedWallBaseElevationMode = new WallElevationModeViewModel(
                _localizationService, settings.WallBaseElevationMode);
            SelectedWallHeightStyle = new WallHeightStyleViewModel(
                _localizationService, settings.WallHeightStyle);
            WallElevationByUser = settings.WallElevationMm.ToString();
            WallBaseOffset = settings.WallBaseOffsetMm.ToString();
            WallSideOffset = settings.WallSideOffsetMm.ToString();
            SelectedWallType = WallTypes.FirstOrDefault(wtvm => wtvm.WallTypeId == settings.WallTypeId);

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);
            settings.RoomGetterMode = SelectedRoomGetterMode.RoomGetterMode;
            settings.WallTopElevationMode = SelectedWallTopElevationMode.ElevationMode;
            settings.WallBaseElevationMode = SelectedWallBaseElevationMode.ElevationMode;
            settings.WallHeightStyle = SelectedWallHeightStyle.WallHeightStyle;
            settings.WallBaseOffsetMm = double.TryParse(WallBaseOffset, out double baseOffset) ? baseOffset : 0;
            settings.WallSideOffsetMm = double.TryParse(WallSideOffset, out double sideOffset) ? sideOffset : 0;
            settings.WallElevationMm = double.TryParse(WallElevationByUser, out double height) ? height : 0;
            settings.WallTypeId = SelectedWallType.WallTypeId;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
