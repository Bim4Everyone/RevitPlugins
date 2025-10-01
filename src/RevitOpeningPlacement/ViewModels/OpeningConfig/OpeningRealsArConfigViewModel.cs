using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class OpeningRealsArConfigViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    public OpeningRealsArConfigViewModel(RevitRepository revitRepository, OpeningRealsArConfig openingRealsArConfig) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        if(openingRealsArConfig is null) {
            throw new ArgumentNullException(nameof(openingRealsArConfig));
        }

        RoundElevation = openingRealsArConfig.ElevationRounding > 0;
        SelectedRoundElevation = openingRealsArConfig.ElevationRounding;
        RoundSize = openingRealsArConfig.Rounding > 0;
        SelectedRoundSize = openingRealsArConfig.Rounding;

        SaveConfigCommand = RelayCommand.Create(SaveConfig);
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


    private OpeningRealsArConfig GetOpeningConfig() {
        var config = OpeningRealsArConfig.GetOpeningConfig(_revitRepository.Doc);
        config.Rounding = SelectedRoundSize;
        config.ElevationRounding = SelectedRoundElevation;
        return config;
    }
}
