using System;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Utils;

internal class NameEditorViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly string[] _existingNames;
    private string _errorText;
    private string _name;
    private string _title;

    public NameEditorViewModel(ILocalizationService localization, string[] existingNames) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _existingNames = existingNames ?? [];

        Create = RelayCommand.Create(() => { }, CanCreate);
    }

    public ICommand Create { get; }

    public string Title {
        get => _title;
        set => RaiseAndSetIfChanged(ref _title, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private bool CanCreate() {
        if(string.IsNullOrWhiteSpace(Name)) {
            ErrorText = _localization.GetLocalizedString("NameEditor.Validation.EmptyName");
            return false;
        }

        if(_existingNames.Contains(Name)) {
            ErrorText = _localization.GetLocalizedString("NameEditor.Validation.DuplicatedName");
            return false;
        }

        ErrorText = null;
        return true;
    }
}
