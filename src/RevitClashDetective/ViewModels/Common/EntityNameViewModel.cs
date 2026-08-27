using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.Common;

/// <summary>
/// Модель представления окна для ввода имени какой-либо именованной сущности:
/// поискового набора, проверки на коллизии и т.д.
/// </summary>
internal class EntityNameViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly List<string> _existingNames;
    private string _name;
    private string _errorText;

    /// <param name="localization">Сервис локализации</param>
    /// <param name="existingNames">Уже занятые имена</param>
    /// <param name="currentName">Текущее имя сущности, если оно есть</param>
    public EntityNameViewModel(
        ILocalizationService localization,
        IEnumerable<string> existingNames,
        string currentName = null) {

        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        if(existingNames is null) { throw new ArgumentNullException(nameof(existingNames)); }
        _existingNames = existingNames.ToList();
        Name = currentName;

        AcceptCommand = RelayCommand.Create(() => { }, CanAccept);
    }

    public ICommand AcceptCommand { get; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private bool CanAccept() {
        if(string.IsNullOrWhiteSpace(Name)) {
            ErrorText = _localization.GetLocalizedString("EntityNameView.Validation.EmptyName");
            return false;
        }

        if(_existingNames.Contains(Name, StringComparer.CurrentCultureIgnoreCase)) {
            ErrorText = _localization.GetLocalizedString("EntityNameView.Validation.DuplicatedName");
            return false;
        }
        ErrorText = null;
        return true;
    }
}
