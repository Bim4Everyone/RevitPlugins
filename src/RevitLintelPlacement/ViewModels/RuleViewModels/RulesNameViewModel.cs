using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels.RuleViewModels;

internal class RulesNameViewModel : BaseViewModel {
    private readonly string _currentName;

    private readonly List<string> _oldFilterNames;
    private string _errorText;
    private string _name;

    public RulesNameViewModel(IEnumerable<string> oldFilterNames) {
        _oldFilterNames = oldFilterNames.ToList();

        Create = new RelayCommand(p => { }, CanCreate);
    }

    public RulesNameViewModel(IEnumerable<string> oldFilterNames, string currentName) {
        _oldFilterNames = oldFilterNames.ToList();
        Name = currentName;
        _currentName = currentName;

        Create = new RelayCommand(p => { }, CanCreate);
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

    private bool CanCreate(object p) {
        if(string.IsNullOrEmpty(Name)) {
            ErrorText = "Введите имя файла.";
            return false;
        }

        if(_oldFilterNames.Contains(Name)
           || Name == _currentName) {
            ErrorText = "Файл с таким именем уже существует.";
            return false;
        }

        ErrorText = null;
        return true;
    }
}
