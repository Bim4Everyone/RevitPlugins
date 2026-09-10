using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class OpeningRealsKrConfigViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Значение минимального расстояния между отверстиями в мм,
    /// подставляемое при включении проверки
    /// </summary>
    private const int _defaultMinDistance = 50;

    public OpeningRealsKrConfigViewModel(
        RevitRepository revitRepository,
        OpeningRealsKrConfig openingRealsKrConfig,
        ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        if(openingRealsKrConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsKrConfig));
        }

        PlaceByMep = openingRealsKrConfig.PlacementType == OpeningRealKrPlacementType.PlaceByMep;
        PlaceByAr = !PlaceByMep;
        RoundElevation = openingRealsKrConfig.ElevationRounding > 0;
        SelectedRoundElevation = openingRealsKrConfig.ElevationRounding;
        RoundSize = openingRealsKrConfig.Rounding > 0;
        SelectedRoundSize = openingRealsKrConfig.Rounding;
        MinDistance = openingRealsKrConfig.MinDistanceBetweenOpenings.ToString(CultureInfo.InvariantCulture);
        CheckDistance = openingRealsKrConfig.MinDistanceBetweenOpenings > 0;

        SaveConfigCommand = RelayCommand.Create(SaveConfig, CanSaveConfig);
    }


    private bool _placeByMep;

    public bool PlaceByMep {
        get => _placeByMep;
        set => RaiseAndSetIfChanged(ref _placeByMep, value);
    }


    private bool _placeByAr;

    public bool PlaceByAr {
        get => _placeByAr;
        set => RaiseAndSetIfChanged(ref _placeByAr, value);
    }

    private bool _roundSize;

    /// <summary>
    /// Включает/выключает округление размеров
    /// </summary>
    public bool RoundSize {
        get => _roundSize;
        set {
            RaiseAndSetIfChanged(ref _roundSize, value);
            SelectedRoundSize = value ? EnabledRoundings.First() : 0;
        }
    }

    private bool _roundElevation;

    /// <summary>
    /// Включает/выключает округление отметки
    /// </summary>
    public bool RoundElevation {
        get => _roundElevation;
        set {
            RaiseAndSetIfChanged(ref _roundElevation, value);
            SelectedRoundElevation = value ? EnabledRoundings.First() : 0;
        }
    }

    private int _selectedRoundElevation;

    /// <summary>
    /// Округление высотной отметки в мм
    /// </summary>
    public int SelectedRoundElevation {
        get => _selectedRoundElevation;
        set => RaiseAndSetIfChanged(ref _selectedRoundElevation, value);
    }

    private int _selectedRoundSize;

    /// <summary>
    /// Округление размеров в мм
    /// </summary>
    public int SelectedRoundSize {
        get => _selectedRoundSize;
        set => RaiseAndSetIfChanged(ref _selectedRoundSize, value);
    }

    private bool _checkDistance;

    /// <summary>
    /// Включает/выключает проверку расстояния между чистовыми отверстиями
    /// </summary>
    public bool CheckDistance {
        get => _checkDistance;
        set {
            RaiseAndSetIfChanged(ref _checkDistance, value);
            if(!value) {
                MinDistance = "0";
            } else if(TryGetMinDistance(out int minDistance)
                      && (minDistance == 0)) {
                MinDistance = _defaultMinDistance.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    private string _minDistance;

    /// <summary>
    /// Минимальное расстояние между чистовыми отверстиями в мм
    /// </summary>
    public string MinDistance {
        get => _minDistance;
        set => RaiseAndSetIfChanged(ref _minDistance, value);
    }

    private string _errorText;

    /// <summary>
    /// Текст ошибки валидации настроек
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Доступные для выбора значения округления в мм
    /// </summary>
    public IReadOnlyCollection<int> EnabledRoundings { get; } = new int[] { 1, 5, 10, 25, 50 };


    public ICommand SaveConfigCommand { get; }

    private void SaveConfig() {
        GetOpeningConfig().SaveProjectConfig();
    }

    private bool CanSaveConfig() {
        if(!TryGetMinDistance(out int minDistance)) {
            ErrorText = _localization.GetLocalizedString("OpeningRealsKrSettingsView.Validation.CannotParseInt");
            return false;
        }

        if((minDistance < 0)
           || (minDistance > OpeningRealsKrConfig.MaxDistanceBetweenOpenings)) {
            ErrorText = _localization.GetLocalizedString(
                "OpeningRealsKrSettingsView.Validation.DistanceRange",
                OpeningRealsKrConfig.MaxDistanceBetweenOpenings);
            return false;
        }

        ErrorText = null;
        return true;
    }

    private bool TryGetMinDistance(out int minDistance) {
        return int.TryParse(MinDistance, NumberStyles.Integer, CultureInfo.InvariantCulture, out minDistance);
    }


    private OpeningRealsKrConfig GetOpeningConfig() {
        var config = OpeningRealsKrConfig.GetOpeningConfig(_revitRepository.Doc);
        config.PlacementType = PlaceByAr
            ? OpeningRealKrPlacementType.PlaceByAr
            : OpeningRealKrPlacementType.PlaceByMep;
        config.Rounding = SelectedRoundSize;
        config.ElevationRounding = SelectedRoundElevation;
        config.MinDistanceBetweenOpenings = CheckDistance && TryGetMinDistance(out int minDistance)
            ? minDistance
            : 0;
        return config;
    }
}
