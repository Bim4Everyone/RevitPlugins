using System;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Настройки статусов Навигатора для файла ВИС
/// </summary>
internal class MepNavigatorSettingsViewModel : BaseViewModel {
    private bool _checkManuallyPlaced;
    private bool _checkNotActual;
    private bool _checkDifferentConstructions;
    private bool _checkUnacceptableConstructions;
    private bool _checkIntersects;
    private bool _checkUnited;
    private bool _checkOffsets;

    public MepNavigatorSettingsViewModel(Models.Configs.OpeningConfig openingConfig) {
        if(openingConfig is null) {
            throw new ArgumentNullException(nameof(openingConfig));
        }

        var settings = openingConfig.NavigatorSettings;
        _checkManuallyPlaced = settings.CheckManuallyPlaced;
        _checkNotActual = settings.CheckNotActual;
        _checkDifferentConstructions = settings.CheckDifferentConstructions;
        _checkUnacceptableConstructions = settings.CheckUnacceptableConstructions;
        _checkIntersects = settings.CheckIntersects;
        _checkUnited = settings.CheckUnited;
        _checkOffsets = settings.CheckOffsets;
    }

    /// <summary>
    /// Включает проверку того, что задание размещено вручную
    /// </summary>
    public bool CheckManuallyPlaced {
        get => _checkManuallyPlaced;
        set => RaiseAndSetIfChanged(ref _checkManuallyPlaced, value);
    }

    /// <summary>
    /// Включает проверку актуальности задания.
    /// <para>
    /// Родительская настройка: ее выключение гасит <see cref="CheckDifferentConstructions"/>,
    /// потому что без нее не собираются конструкции-основы, на которых та работает.
    /// </para>
    /// </summary>
    public bool CheckNotActual {
        get => _checkNotActual;
        set {
            if(_checkNotActual == value) {
                return;
            }

            RaiseAndSetIfChanged(ref _checkNotActual, value);
            if(!value) {
                CheckDifferentConstructions = false;
            }
        }
    }

    /// <summary>
    /// Включает проверку расположения задания в конструкциях разных категорий.
    /// <para>Не может быть включена без <see cref="CheckNotActual"/>.</para>
    /// </summary>
    public bool CheckDifferentConstructions {
        get => _checkDifferentConstructions;
        set {
            if(_checkDifferentConstructions == value) {
                return;
            }

            RaiseAndSetIfChanged(ref _checkDifferentConstructions, value);
            if(value) {
                CheckNotActual = true;
            }
        }
    }

    /// <summary>
    /// Включает проверку расположения задания в недопустимых конструкциях
    /// </summary>
    public bool CheckUnacceptableConstructions {
        get => _checkUnacceptableConstructions;
        set => RaiseAndSetIfChanged(ref _checkUnacceptableConstructions, value);
    }

    /// <summary>
    /// Включает проверку пересечения задания с другими заданиями
    /// </summary>
    public bool CheckIntersects {
        get => _checkIntersects;
        set => RaiseAndSetIfChanged(ref _checkIntersects, value);
    }

    /// <summary>
    /// Включает проверку того, что задание объединенное
    /// </summary>
    public bool CheckUnited {
        get => _checkUnited;
        set => RaiseAndSetIfChanged(ref _checkUnited, value);
    }

    /// <summary>
    /// Включает проверку отступов задания от элемента инженерных систем
    /// </summary>
    public bool CheckOffsets {
        get => _checkOffsets;
        set => RaiseAndSetIfChanged(ref _checkOffsets, value);
    }

    /// <summary>
    /// Записывает текущие настройки статусов Навигатора в конфиг
    /// </summary>
    public void UpdateConfig(Models.Configs.OpeningConfig config) {
        var settings = config.NavigatorSettings;
        settings.CheckManuallyPlaced = CheckManuallyPlaced;
        settings.CheckNotActual = CheckNotActual;
        settings.CheckDifferentConstructions = CheckDifferentConstructions;
        settings.CheckUnacceptableConstructions = CheckUnacceptableConstructions;
        settings.CheckIntersects = CheckIntersects;
        settings.CheckUnited = CheckUnited;
        settings.CheckOffsets = CheckOffsets;
    }
}
