using System;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;

/// <summary>
/// Настройки статусов входящих заданий на отверстия, общие для файлов АР и КР.
/// </summary>
internal class IncomingStatusesViewModel : BaseViewModel {
    private bool _checkUnacceptableConstructions;
    private bool _checkIntersections;
    private bool _checkDifferentConstructions;
    private bool _checkHost;

    public IncomingStatusesViewModel(IncomingTaskStatusesSettings settings) {
        if(settings is null) {
            throw new ArgumentNullException(nameof(settings));
        }

        _checkUnacceptableConstructions = settings.CheckUnacceptableConstructions;
        _checkIntersections = settings.CheckIntersections;
        _checkDifferentConstructions = settings.CheckDifferentConstructions;
        _checkHost = settings.CheckHost;
    }

    /// <summary>
    /// Включает проверку расположения задания в недопустимых конструкциях
    /// </summary>
    public bool CheckUnacceptableConstructions {
        get => _checkUnacceptableConstructions;
        set => RaiseAndSetIfChanged(ref _checkUnacceptableConstructions, value);
    }

    /// <summary>
    /// Включает сопоставление задания с конструкциями и чистовыми отверстиями.
    /// <para>
    /// Родительская настройка: ее выключение гасит <see cref="CheckDifferentConstructions"/>
    /// и <see cref="CheckHost"/>, потому что без нее не собираются данные, на которых те работают.
    /// </para>
    /// </summary>
    public bool CheckIntersections {
        get => _checkIntersections;
        set {
            if(_checkIntersections == value) {
                return;
            }

            RaiseAndSetIfChanged(ref _checkIntersections, value);
            if(!value) {
                CheckDifferentConstructions = false;
                CheckHost = false;
            }
        }
    }

    /// <summary>
    /// Включает проверку расположения задания в конструкциях разных категорий.
    /// <para>Не может быть включена без <see cref="CheckIntersections"/>.</para>
    /// </summary>
    public bool CheckDifferentConstructions {
        get => _checkDifferentConstructions;
        set {
            if(_checkDifferentConstructions == value) {
                return;
            }

            RaiseAndSetIfChanged(ref _checkDifferentConstructions, value);
            if(value) {
                CheckIntersections = true;
            }
        }
    }

    /// <summary>
    /// Включает поиск основы задания на отверстие.
    /// <para>Не может быть включен без <see cref="CheckIntersections"/>.</para>
    /// </summary>
    public bool CheckHost {
        get => _checkHost;
        set {
            if(_checkHost == value) {
                return;
            }

            RaiseAndSetIfChanged(ref _checkHost, value);
            if(value) {
                CheckIntersections = true;
            }
        }
    }

    /// <summary>
    /// Записывает текущие настройки статусов входящих заданий в конфиг
    /// </summary>
    public void UpdateConfig(IncomingTaskStatusesSettings settings) {
        if(settings is null) {
            throw new ArgumentNullException(nameof(settings));
        }

        settings.CheckUnacceptableConstructions = CheckUnacceptableConstructions;
        settings.CheckIntersections = CheckIntersections;
        settings.CheckDifferentConstructions = CheckDifferentConstructions;
        settings.CheckHost = CheckHost;
    }
}
