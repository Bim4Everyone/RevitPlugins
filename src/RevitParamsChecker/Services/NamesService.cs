using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitParamsChecker.Models;
using RevitParamsChecker.ViewModels.Utils;
using RevitParamsChecker.Views.Utils;

namespace RevitParamsChecker.Services;

internal class NamesService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly ILocalizationService _localization;
    private readonly IEqualityComparer<IName> _comparer;

    public NamesService(IResolutionRoot resolutionRoot, ILocalizationService localization) {
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _comparer = new NamesIgnoreCaseComparer();
    }

    /// <summary>
    /// Возвращает коллекцию выбранных названий из доступных с учетом начального выбора
    /// </summary>
    /// <param name="prompt">Описание выбираемых названий</param>
    /// <param name="allNames">Все доступные названия</param>
    /// <param name="selectedNames">Начальный выбор названий</param>
    /// <returns>Коллекция выбранных названий</returns>
    /// <exception cref="System.OperationCanceledException">Исключение, если операция была отменена</exception>
    public async Task<ICollection<string>> SelectNamesAsync(string prompt, string[] allNames, string[] selectedNames) {
        var allNamesArg = new ConstructorArgument(nameof(allNames), allNames);
        var selectedNamesArg = new ConstructorArgument(nameof(selectedNames), selectedNames);
        var vm = _resolutionRoot.Get<SelectableNamesViewModel>(allNamesArg, selectedNamesArg);
        vm.Title = prompt;

        var dialog = _resolutionRoot.Get<SelectableNamesDialog>();
        dialog.DataContext = vm;
        var dialogResult = await dialog.ShowAsync();
        if(dialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary) {
            return vm.GetSelectedEntities();
        } else {
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// Создает новое название, которое не еще не существует
    /// </summary>
    /// <param name="prompt">Описание названия</param>
    /// <param name="existingNames">Существующие названия</param>
    /// <returns>Новое название</returns>
    /// <exception cref="System.OperationCanceledException">Исключение, если операция была отменена</exception>
    public string CreateNewName(string prompt, string[] existingNames) {
        var existingNamesArg = new ConstructorArgument(nameof(existingNames), existingNames);
        var vm = _resolutionRoot.Get<NameEditorViewModel>(existingNamesArg);
        vm.Title = prompt;

        var window = _resolutionRoot.Get<NameEditorWindow>();
        window.DataContext = vm;
        if(window.ShowDialog() == true) {
            return vm.Name;
        } else {
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// Создает новое название, которое не еще не существует
    /// </summary>
    /// <param name="prompt">Описание названия</param>
    /// <param name="existingNames">Существующие названия</param>
    /// <param name="currentName">Начальное название</param>
    /// <returns>Новое название</returns>
    /// <exception cref="System.OperationCanceledException">Исключение, если операция была отменена</exception>
    public string CreateNewName(string prompt, string[] existingNames, string currentName) {
        var existingNamesArg = new ConstructorArgument(nameof(existingNames), existingNames);
        var vm = _resolutionRoot.Get<NameEditorViewModel>(existingNamesArg);
        vm.Title = prompt;
        vm.Name = currentName;

        var window = _resolutionRoot.Get<NameEditorWindow>();
        window.DataContext = vm;
        if(window.ShowDialog() == true) {
            return vm.Name;
        } else {
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// При конфликте имен в коллекциях решает этот конфликт и возвращает итоговую коллекцию
    /// </summary>
    /// <param name="oldEntities">Старая коллекция</param>
    /// <param name="newEntities">Новая коллекция</param>
    /// <returns>Коллекция с решенным конфликтом имен</returns>
    /// <exception cref="System.OperationCanceledException">Исключение, если операция была отменена</exception>
    public ICollection<IName> GetResolvedCollection(
        ICollection<IName> oldEntities,
        ICollection<IName> newEntities) {
        string[] intersections = newEntities
            .Intersect(oldEntities, _comparer)
            .Select(r => r.Name)
            .ToArray();
        if(intersections.Length > 0) {
            switch(GetResult(string.Join(", ", intersections))) {
                case TaskDialogResult.CommandLink1: {
                    return Replace(oldEntities, newEntities);
                }
                case TaskDialogResult.CommandLink2: {
                    return KeepOnlyOld(oldEntities, newEntities);
                }
                case TaskDialogResult.CommandLink3: {
                    return CopyAndRename(oldEntities, newEntities);
                }
                case TaskDialogResult.Cancel: {
                    throw new OperationCanceledException();
                }
                default: {
                    return oldEntities;
                }
            }
        } else {
            return oldEntities.Union(newEntities).ToArray();
        }
    }

    private TaskDialogResult GetResult(string names) {
        var dialog = new TaskDialog(_localization.GetLocalizedString("NamesResolver.Header")) {
            MainContent = _localization.GetLocalizedString("NamesResolver.Body", names)
        };
        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink1,
            _localization.GetLocalizedString("NamesResolver.Replace"));
        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink2,
            _localization.GetLocalizedString("NamesResolver.KeepOnlyOld"));
        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink3,
            _localization.GetLocalizedString("NamesResolver.CopyAndRename"));
        dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
        return dialog.Show();
    }

    private ICollection<IName> Replace(
        ICollection<IName> oldEntities,
        ICollection<IName> addedEntities) {
        return oldEntities
            .Except(addedEntities, _comparer)
            .Union(addedEntities)
            .ToArray();
    }

    private ICollection<IName> KeepOnlyOld(
        ICollection<IName> oldEntities,
        ICollection<IName> addedEntities) {
        return oldEntities
            .Union(addedEntities.Except(oldEntities, _comparer))
            .ToArray();
    }

    private ICollection<IName> CopyAndRename(
        ICollection<IName> oldEntities,
        ICollection<IName> addedEntities) {
        var intersection = addedEntities
            .Intersect(oldEntities, _comparer)
            .ToArray();
        var renamedIntersection = GetRenamedEntities(oldEntities, intersection).ToArray();
        return oldEntities
            .Union(renamedIntersection)
            .Union(addedEntities.Except(intersection, _comparer))
            .ToArray();
    }

    private IEnumerable<IName> GetRenamedEntities(
        ICollection<IName> oldEntities,
        ICollection<IName> intersection) {
        foreach(var element in intersection) {
            int number = oldEntities.Where(item => item.Name.StartsWith(element.Name))
                .Select(item => GetNameNumber(item.Name, element.Name))
                .Max();
            element.Name += number / 10 > 0 ? $"_{number + 1}" : $"_0{number + 1}";
            yield return element;
        }
    }

    private int GetNameNumber(string name, string addedElementName) {
        return name.Length == addedElementName.Length
            ? 0
            : int.TryParse(name.Substring(addedElementName.Length + 1), out int res)
                ? res
                : 0;
    }
}
