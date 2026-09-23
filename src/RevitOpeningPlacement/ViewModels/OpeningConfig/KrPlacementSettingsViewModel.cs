using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

internal class KrPlacementSettingsViewModel : BaseViewModel {
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

    /// <summary>
    /// Доступные для выбора значения округления в мм
    /// </summary>
    public IReadOnlyCollection<int> EnabledRoundings { get; } = new int[] { 1, 5, 10, 25, 50 };

    private string _errorText;

    /// <summary>
    /// Текст ошибки валидации страницы.
    /// <para>Настройки расстановки этого раздела вводятся выбором из списка, ошибок не бывает.</para>
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    } // TODO отрефакторить ErrorText в 3-х окнах

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
