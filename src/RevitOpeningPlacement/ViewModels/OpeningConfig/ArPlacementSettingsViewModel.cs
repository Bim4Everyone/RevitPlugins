using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

internal class ArPlacementSettingsViewModel : BaseViewModel {
    private bool _roundSize;
    private bool _roundElevation;
    private int _selectedRoundElevation;
    private int _selectedRoundSize;

    public ArPlacementSettingsViewModel(OpeningRealsArConfig openingRealsArConfig) {
        if(openingRealsArConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsArConfig));
        }

        RoundElevation = openingRealsArConfig.ElevationRounding > 0;
        SelectedRoundElevation = openingRealsArConfig.ElevationRounding;
        RoundSize = openingRealsArConfig.Rounding > 0;
        SelectedRoundSize = openingRealsArConfig.Rounding;
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
    public void UpdateConfig(OpeningRealsArConfig config) {
        config.Rounding = SelectedRoundSize;
        config.ElevationRounding = SelectedRoundElevation;
    }
}
