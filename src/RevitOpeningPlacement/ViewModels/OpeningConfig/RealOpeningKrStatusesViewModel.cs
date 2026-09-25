using System;

using dosymep.SimpleServices;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Настройки статусов чистовых отверстий КР.
/// <para>Добавляет к общим настройкам проверку расстояния между отверстиями.</para>
/// </summary>
internal class RealOpeningKrStatusesViewModel : RealOpeningArStatusesViewModel {
    private readonly RealOpeningKrStatusesSettings _settings;
    private readonly ILocalizationService _localization;
    private bool _checkTooClose;
    private int _minDistance;

    public RealOpeningKrStatusesViewModel(
        RealOpeningKrStatusesSettings settings,
        ILocalizationService localization)
        : base(settings, localization) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        _checkTooClose = _settings.CheckTooClose;
        _minDistance = _settings.MinDistanceBetweenOpenings;
    }

    /// <summary>
    /// Включает проверку расстояния между чистовыми отверстиями (статус TooClose)
    /// </summary>
    public bool CheckTooClose {
        get => _checkTooClose;
        set => RaiseAndSetIfChanged(ref _checkTooClose, value);
    }

    /// <summary>
    /// Минимальное расстояние между чистовыми отверстиями в мм.
    /// </summary>
    public int MinDistance {
        get => _minDistance;
        set => RaiseAndSetIfChanged(ref _minDistance, value);
    }

    protected override bool CanAcceptView() {
        if(!base.CanAcceptView()) {
            return false;
        }

        if((MinDistance < 0)
           || (MinDistance > _settings.MaxDistanceBetweenOpenings)) {
            ErrorText = _localization.GetLocalizedString(
                "NavigatorSettings.Validation.DistanceRange",
                _settings.MaxDistanceBetweenOpenings);
            return false;
        }

        ErrorText = null;
        return true;
    }

    public void UpdateConfig(RealOpeningKrStatusesSettings settings) {
        base.UpdateConfig(settings);

        settings.CheckTooClose = CheckTooClose;
        settings.MinDistanceBetweenOpenings = MinDistance;
    }
}
