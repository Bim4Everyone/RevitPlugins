using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class FilterNameViewModel : BaseViewModel {
    private string _name;

    private readonly List<string> _oldFilterNames;
    private string _errorText;
    private readonly string _currentName;
    private readonly ILocalizationService _localization;

    public FilterNameViewModel(ILocalizationService localization, IEnumerable<string> oldFilterNames) {
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        _oldFilterNames = oldFilterNames.ToList();

        Create = RelayCommand.Create(() => { }, CanCreate);
    }

    public FilterNameViewModel(ILocalizationService localization, IEnumerable<string> oldFilterNames, string currentName) {
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        _oldFilterNames = oldFilterNames.ToList();
        Name = currentName;
        _currentName = currentName;

        Create = RelayCommand.Create(() => { }, CanCreate);
    }

    public ICommand Create { get; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private bool CanCreate() {
        if(string.IsNullOrEmpty(Name)) {
            ErrorText = _localization.GetLocalizedString("FilterCreation.Validation.SetName");
            return false;
        }

        if(_oldFilterNames.Contains(Name) || Name == _currentName) {
            ErrorText = _localization.GetLocalizedString("FilterCreation.Validation.DuplicatedName");
            return false;
        }
        ErrorText = null;
        return true;
    }
}
