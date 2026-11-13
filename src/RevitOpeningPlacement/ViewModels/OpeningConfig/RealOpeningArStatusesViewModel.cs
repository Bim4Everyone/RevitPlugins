using System;
using System.Globalization;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Настройки статусов чистовых отверстий.
/// <para>Общие для файлов АР и КР, поэтому вынесены в отдельную вью модель.</para>
/// </summary>
internal class RealOpeningArStatusesViewModel : BaseViewModel {
    private readonly IRealOpeningStatusesSettings _settings;
    private readonly ILocalizationService _localization;
    private bool _checkNotActual;
    private bool _checkVolumeMatch;
    private double _emptyVolumeRatio;
    private double _tooBigVolumeRatio;
    private string _errorText;

    public RealOpeningArStatusesViewModel(
        IRealOpeningStatusesSettings settings,
        ILocalizationService localization) {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        _checkNotActual = _settings.CheckNotActual;
        _checkVolumeMatch = _settings.CheckVolumeMatch;
        _emptyVolumeRatio = _settings.EmptyVolumeRatio;
        _tooBigVolumeRatio = _settings.TooBigVolumeRatio;

        CanAcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
    }

    /// <summary>
    /// Команда-заглушка для отображения ошибок валидации в <see cref="ErrorText"/>
    /// </summary>
    public ICommand CanAcceptViewCommand { get; }

    /// <summary>
    /// Включает проверку актуальности чистового отверстия
    /// </summary>
    public bool CheckNotActual {
        get => _checkNotActual;
        set {
            RaiseAndSetIfChanged(ref _checkNotActual, value);
            if(!value) {
                CheckVolumeMatch = false;
            }
        }
    }

    /// <summary>
    /// Включает проверку соответствия объема чистового отверстия элементам из связей.
    /// Можно включить, только если включена <see cref="CheckNotActual"/>
    /// </summary>
    public bool CheckVolumeMatch {
        get => _checkVolumeMatch;
        set {
            RaiseAndSetIfChanged(ref _checkVolumeMatch, value);
            if(value) {
                CheckNotActual = true;
            }
        }
    }

    /// <summary>
    /// Доля от 1 пересеченного объема, ниже которого отверстие считается пустым
    /// </summary>
    public double EmptyVolumeRatio {
        get => _emptyVolumeRatio;
        set => RaiseAndSetIfChanged(ref _emptyVolumeRatio, value);
    }

    /// <summary>
    /// Доля от 1 пересеченного объема, ниже которого отверстие считается слишком большим
    /// </summary>
    public double TooBigVolumeRatio {
        get => _tooBigVolumeRatio;
        set => RaiseAndSetIfChanged(ref _tooBigVolumeRatio, value);
    }

    /// <summary>
    /// Текст ошибки валидации настроек чистовых отверстий
    /// </summary>
    public string ErrorText {
        get => _errorText;
        protected set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Проверяет корректность введенных настроек, заполняя <see cref="ErrorText"/> при ошибке
    /// </summary>
    protected virtual bool CanAcceptView() {
        if((EmptyVolumeRatio < _settings.MinEmptyVolumeRatio)
           || (EmptyVolumeRatio > _settings.MaxEmptyVolumeRatio)) {
            ErrorText = _localization.GetLocalizedString(
                "NavigatorSettings.Validation.EmptyVolumeRange",
                _settings.MinEmptyVolumeRatio,
                _settings.MaxEmptyVolumeRatio);
            return false;
        }

        if((TooBigVolumeRatio <= EmptyVolumeRatio)
           || (TooBigVolumeRatio > _settings.MaxTooBigVolumeRatio)) {
            ErrorText = _localization.GetLocalizedString(
                "NavigatorSettings.Validation.TooBigVolumeRange",
                EmptyVolumeRatio,
                _settings.MaxTooBigVolumeRatio);
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Записывает текущие настройки статусов чистовых отверстий в конфиг
    /// </summary>
    public void UpdateConfig(IRealOpeningStatusesSettings settings) {
        if(settings is null) {
            throw new ArgumentNullException(nameof(settings));
        }

        settings.CheckNotActual = CheckNotActual;
        settings.CheckVolumeMatch = CheckVolumeMatch;
        settings.EmptyVolumeRatio = EmptyVolumeRatio;
        settings.TooBigVolumeRatio = TooBigVolumeRatio;
    }
}
