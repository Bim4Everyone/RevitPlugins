using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class OpeningRealsKrConfigViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    public OpeningRealsKrConfigViewModel(RevitRepository revitRepository, OpeningRealsKrConfig openingRealsKrConfig) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        if(openingRealsKrConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsKrConfig));
        }

        PlaceByMep = openingRealsKrConfig.PlacementType == OpeningRealKrPlacementType.PlaceByMep;
        PlaceByAr = !PlaceByMep;
        RoundElevation = openingRealsKrConfig.ElevationRounding > 0;
        SelectedRoundElevation = openingRealsKrConfig.ElevationRounding;
        RoundSize = openingRealsKrConfig.Rounding > 0;
        SelectedRoundSize = openingRealsKrConfig.Rounding;

        SaveConfigCommand = RelayCommand.Create(SaveConfig);
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


    public ICommand SaveConfigCommand { get; }

    private void SaveConfig() {
        GetOpeningConfig().SaveProjectConfig();
    }


    private OpeningRealsKrConfig GetOpeningConfig() {
        var config = OpeningRealsKrConfig.GetOpeningConfig(_revitRepository.Doc);
        config.PlacementType = PlaceByAr
            ? OpeningRealKrPlacementType.PlaceByAr
            : OpeningRealKrPlacementType.PlaceByMep;
        config.Rounding = SelectedRoundSize;
        config.ElevationRounding = SelectedRoundElevation;
        return config;
    }
}
