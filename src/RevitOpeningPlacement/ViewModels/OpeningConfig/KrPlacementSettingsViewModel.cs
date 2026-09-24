using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

internal class KrPlacementSettingsViewModel : BaseViewModel {
    private bool _placeByMep;
    private bool _placeByAr;
    private bool _roundSize;
    private bool _roundElevation;
    private int _selectedRoundElevation;
    private int _selectedRoundSize;

    public KrPlacementSettingsViewModel(OpeningRealsKrConfig openingRealsKrConfig) {
        if(openingRealsKrConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsKrConfig));
        }

        PlaceByMep = openingRealsKrConfig.PlacementType == OpeningRealKrPlacementType.PlaceByMep;
        PlaceByAr = !PlaceByMep;
        RoundElevation = openingRealsKrConfig.ElevationRounding > 0;
        SelectedRoundElevation = openingRealsKrConfig.ElevationRounding;
        RoundSize = openingRealsKrConfig.Rounding > 0;
        SelectedRoundSize = openingRealsKrConfig.Rounding;
    }

    public bool PlaceByMep {
        get => _placeByMep;
        set => RaiseAndSetIfChanged(ref _placeByMep, value);
    }

    public bool PlaceByAr {
        get => _placeByAr;
        set => RaiseAndSetIfChanged(ref _placeByAr, value);
    }

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

    /// <summary>
    /// Округление высотной отметки в мм
    /// </summary>
    public int SelectedRoundElevation {
        get => _selectedRoundElevation;
        set => RaiseAndSetIfChanged(ref _selectedRoundElevation, value);
    }

    /// <summary>
    /// Округление размеров в мм
    /// </summary>
    public int SelectedRoundSize {
        get => _selectedRoundSize;
        set => RaiseAndSetIfChanged(ref _selectedRoundSize, value);
    }

    /// <summary>
    /// Доступные для выбора значения округления в мм
    /// </summary>
    public IReadOnlyCollection<int> EnabledRoundings { get; } = [1, 5, 10, 25, 50];

    /// <summary>
    /// Записывает текущие настройки расстановки в конфиг
    /// </summary>
    public void UpdateConfig(OpeningRealsKrConfig config) {
        config.PlacementType = PlaceByAr
            ? OpeningRealKrPlacementType.PlaceByAr
            : OpeningRealKrPlacementType.PlaceByMep;
        config.Rounding = SelectedRoundSize;
        config.ElevationRounding = SelectedRoundElevation;
    }
}
