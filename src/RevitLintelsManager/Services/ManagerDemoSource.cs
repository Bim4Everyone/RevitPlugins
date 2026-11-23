using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.ViewModels;

namespace RevitLintelsManager.Services;

/// <summary>Временные GUI-данные TASK-006; компилируются только в явно включённую Debug-демонстрацию.</summary>
internal sealed class ManagerDemoSource(ManagerSessionState state, ILocalizationService localization)
    : IManagerRequestDispatcher {
    private const string _secondLevel = "DEMO — 02 этаж +5.700";
    private readonly Dictionary<ManagerGroupKey, bool> _expanded = new();
    private IReadOnlyList<ManagerItemViewModel> _items;
    private IReadOnlyList<ManagerGroupViewModel> _groups = Array.Empty<ManagerGroupViewModel>();
    private ManagerViewModel _viewModel;
    private ManagerScope _scope;
    private bool _accepting;
    private bool _closing;

    /// <summary>Подключает готовые строки и команды диапазонов к существующему менеджеру.</summary>
    /// <param name="viewModel">Единственная VM текущего окна.</param>
    /// <param name="commands">Существующая фабрика безопасных UI-команд.</param>
    public void Connect(ManagerViewModel viewModel, ISettingsCommandFactory commands) {
        _viewModel = viewModel;
        _scope = state.InitialScope;
        _items = CreateItems(55);
        _viewModel.SetScopeCommands(
            commands.Create(() => ChangeScope(ManagerScope.Project), () => viewModel.CanEditRows),
            commands.Create(() => ChangeScope(ManagerScope.View), () => viewModel.CanEditRows),
            commands.Create(() => ChangeScope(ManagerScope.Selected), () => viewModel.CanEditRows));
        PublishGroups();
        _viewModel.SetExpanded(_groups[0], true);
    }

    /// <inheritdoc />
    public void StartAcceptingRequests() {
        _accepting = !_closing;
    }

    /// <inheritdoc />
    public Task<ManagerDocumentContext> Enqueue(ManagerRequest request) {
        if(request is null) {
            throw new ArgumentNullException(nameof(request));
        }
        if(!_accepting || _closing || state.IsClosing || state.IsBlocked
            || request.DocumentToken != state.DocumentToken || request.Generation != state.Generation) {
            return Task.FromCanceled<ManagerDocumentContext>(new System.Threading.CancellationToken(true));
        }
        PublishGroups();
        return Task.FromResult(new ManagerDocumentContext(state.DocumentToken, request.Generation,
            "DEMO", "DEMO-View", localization.GetLocalizedString("Manager." + _scope),
            _scope == ManagerScope.Selected ? _items.Take(2)
                .SelectMany(item => new[] { item.Snapshot.Key.OpeningUniqueId, item.Snapshot.Key.LintelUniqueId })
                .Where(id => id is not null) : Array.Empty<string>()));
    }

    /// <inheritdoc />
    public void InvalidateRequests() {
        // Синхронные независимые ответы не оставляют ожидающих запросов.
    }

    /// <inheritdoc />
    public void BeginClose() {
        _closing = true;
        _accepting = false;
    }

    /// <inheritdoc />
    public void PumpInApiContext() {
        // Демонстрация не создаёт ExternalEvent и не сигнализирует Revit.
    }

    /// <inheritdoc />
    public bool TryDisposeInApiContext() {
        BeginClose();
        return true;
    }

    /// <inheritdoc />
    public void Dispose() {
        BeginClose();
        _viewModel = null;
        _items = null;
        _groups = Array.Empty<ManagerGroupViewModel>();
        _expanded.Clear();
    }

    /// <summary>Формирует тот же набор сценариев с заданным числом проёмов без перемычек.</summary>
    /// <param name="missingLintelCount">Размер увеличиваемой группы для ограниченной проверки виртуализации.</param>
    /// <returns>Готовые строки на согласованных моделях, без элементов Revit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Размер группы меньше одного.</exception>
    public IReadOnlyList<ManagerItemViewModel> CreateItems(int missingLintelCount) {
        if(missingLintelCount < 1) {
            throw new ArgumentOutOfRangeException(nameof(missingLintelCount));
        }
        var items = new List<ManagerItemViewModel>();
        for(int index = 0; index < 5; index++) {
            AddItem(items, "door-900", "2L", ManagerElementStatus.Correct, ManagerElementStatus.Correct,
                ManagerAction.Skip,
                new[] { ManagerAction.Delete, ManagerAction.Skip });
        }
        // Подготовленное ручное назначение воспроизводит «Варианты» на макете; default остаётся Skip.
        items[4].SelectedAction = items[4].Actions.First(option => option.Action == ManagerAction.Delete);
        for(int index = 0; index < missingLintelCount; index++) {
            AddItem(items, "door-1200", null, ManagerElementStatus.Correct, ManagerElementStatus.Missing,
                ManagerAction.Install, new[] { ManagerAction.Install, ManagerAction.Skip },
                rowReasons: new[] { "MissingLintel" });
        }
        for(int index = 0; index < 3; index++) {
            AddItem(items, "door-1800", "1L", ManagerElementStatus.Correct, ManagerElementStatus.Warning,
                ManagerAction.Replace, new[] { ManagerAction.Replace, ManagerAction.Delete, ManagerAction.Skip },
                lintelReasons: new[] { "WrongType" });
        }
        AddItem(items, "door-1400", "2L", ManagerElementStatus.Correct, ManagerElementStatus.Warning,
            ManagerAction.Correct, new[] { ManagerAction.Correct, ManagerAction.Delete, ManagerAction.Skip },
            lintelReasons: new[] { "Offset", "MissingLock" });
        AddItem(items, "door-1400", "2L", ManagerElementStatus.Correct, ManagerElementStatus.Warning,
            ManagerAction.Replace,
            new[] { ManagerAction.Replace, ManagerAction.Correct, ManagerAction.Delete, ManagerAction.Skip },
            lintelReasons: new[] { "Offset", "WrongParameters", "MissingLock" });
        for(int index = 0; index < 2; index++) {
            AddItem(items, "window-1500", "2L", ManagerElementStatus.Correct, ManagerElementStatus.Warning,
                ManagerAction.Lock, new[] { ManagerAction.Lock, ManagerAction.Delete, ManagerAction.Skip },
                lintelReasons: new[] { "MissingLock" });
        }
        AddItem(items, "door-900", "2L", ManagerElementStatus.Fixed, ManagerElementStatus.Correct,
            ManagerAction.Skip, new[] { ManagerAction.Skip }, openingReasons: new[] { "FixedOpening" });
        for(int index = 0; index < 2; index++) {
            AddItem(items, "door-1200", "2L", ManagerElementStatus.Correct, ManagerElementStatus.Fixed,
                ManagerAction.Skip, new[] { ManagerAction.Skip }, lintelReasons: new[] { "FixedLintel" });
        }
        AddItem(items, "door-1800", "2L", ManagerElementStatus.Correct, ManagerElementStatus.Grouped,
            ManagerAction.Skip, new[] { ManagerAction.Skip }, lintelReasons: new[] { "GroupedLintel" });
        AddItem(items, null, "2L", ManagerElementStatus.Missing, ManagerElementStatus.Warning,
            ManagerAction.Delete, new[] { ManagerAction.Delete, ManagerAction.Skip },
            rowReasons: new[] { "MissingOpening" });
        return items.AsReadOnly();
    }

    // Меняет только демонстрационный набор; вид и выделение Revit не читаются и не изменяются.
    private void ChangeScope(ManagerScope scope) {
        if(!_viewModel.RefreshCommand.CanExecute(null)) {
            return;
        }
        _scope = scope;
        _viewModel.SelectedScope = _viewModel.Scopes.First(option => option.Scope == scope);
        _viewModel.RefreshCommand.Execute(null);
    }

    // Повторно использует дочерние VM; все агрегаты вычисляют прежние GroupVM и ManagerVM.
    private void PublishGroups() {
        foreach(var group in _groups) {
            _expanded[group.Key] = group.IsExpanded;
        }
        var visible = _scope == ManagerScope.Project ? _items
            : _scope == ManagerScope.View ? _items.Where(item => item.Opening.Element?.Information.LevelName
                == _secondLevel).ToList()
            : _items.Take(2).ToList();
        _groups = visible.GroupBy(item => item.Snapshot.GroupKey).Select(group => {
            var first = group.First();
            return new ManagerGroupViewModel(group.Key, first.Opening.Element, first.Lintel.Element,
                group.ToList().AsReadOnly(), localization) {
                IsExpanded = _expanded.TryGetValue(group.Key, out bool expanded) && expanded
            };
        }).ToList().AsReadOnly();
        _viewModel.SetGroups(_groups);
    }

    // Явно задаёт результаты сценария, не выполняя диагностику или подбор.
    private void AddItem(List<ManagerItemViewModel> items, string openingType, string lintelType,
        ManagerElementStatus openingStatus, ManagerElementStatus lintelStatus, ManagerAction action,
        ManagerAction[] allowed, string[] openingReasons = null, string[] lintelReasons = null,
        string[] rowReasons = null) {
        int number = items.Count + 1;
        var opening = CreateElement(openingType, openingStatus, number, true, openingReasons);
        var lintel = CreateElement(lintelType, lintelStatus, number, false, lintelReasons);
        var key = new ManagerItemKey(state.DocumentToken, opening is null ? null : $"DEMO-Opening-{number}",
            lintel is null ? null : $"DEMO-Lintel-{number}");
        var groupKey = new ManagerGroupKey(openingType is null ? null : "DEMO-Type-" + openingType,
            lintelType is null ? null : "DEMO-Type-" + lintelType, openingStatus, lintelStatus,
            lintelStatus == ManagerElementStatus.Fixed);
        var snapshot = new ManagerItemSnapshot(key, groupKey, Reasons(rowReasons), allowed, action);
        items.Add(new ManagerItemViewModel(snapshot, opening, lintel, localization));
    }

    // Строковые демонстрационные ID отделены от nullable числового идентификатора Revit.
    private static ManagerElementPresentation CreateElement(string type, ManagerElementStatus status,
        int number, bool isOpening, string[] reasons) {
        if(type is null) {
            return null;
        }
        string typeName = type switch {
            "door-900" => "ДВ Сталь ПП 900x2390 EIS60 О П Д_Лестничная клетка",
            "door-1200" => "ДВ Сталь ПП 1200x2390 EIS60 О П Д_Коридор",
            "door-1400" => "ДВ Сталь ПП 1400x2390 EIS60 О П Д_Техническое помещение",
            "door-1800" => "ДВ Сталь ПП 1800x2390 EIS60 О П Д_Венткамера Насосная",
            "window-1500" => "ОК ПВХ 1500x1500",
            "1L" => "1L - А3, уголковая, 1800x200 мм",
            _ => "2L - А3, уголковая, 1800x200 мм"
        };
        string family = isOpening ? type == "window-1500" ? "Окн_ПВХ" : "Двр_Двр_Двуп"
            : "ОбщМд_Перемычка";
        var info = new ManagerElementInfo("DEMO — " + family,
            number % 3 == 0 ? "DEMO — 02_АР_Перегородки" : "DEMO — 01_АР_Основной",
            number % 2 == 0 ? _secondLevel : "DEMO — 01 этаж +2.400",
            status == ManagerElementStatus.Grouped ? "DEMO — Секция А, типовой этаж" : string.Empty,
            status == ManagerElementStatus.Fixed, null,
            isOpening ? $"DEMO-Opening-{number}" : $"DEMO-Lintel-{number}");
        return new ManagerElementPresentation("DEMO — " + typeName, status, info, Reasons(reasons));
    }

    // Все предупреждения используют согласованный контракт и парные RU/EN ресурсы.
    private static IEnumerable<ManagerDiagnostic> Reasons(IEnumerable<string> reasons) {
        return (reasons ?? Array.Empty<string>())
            .Select(reason => new ManagerDiagnostic("Manager.Demo." + reason, Array.Empty<string>()));
    }
}
