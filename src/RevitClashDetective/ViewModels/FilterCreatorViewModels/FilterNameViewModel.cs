using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FilterNameViewModel : BaseViewModel {
        private string _name;

        private readonly List<string> _oldFilterNames;
        private string _errorText;
        private readonly string _currentName;

        public FilterNameViewModel(IEnumerable<string> oldFilterNames) {
            _oldFilterNames = oldFilterNames.ToList();

            Create = RelayCommand.Create(() => { }, CanCreate);
        }

        public FilterNameViewModel(IEnumerable<string> oldFilterNames, string currentName) {
            _oldFilterNames = oldFilterNames.ToList();
            Name = currentName;
            _currentName = currentName;

            Create = RelayCommand.Create(() => { }, CanCreate);
        }

        public ICommand Create { get; }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanCreate() {
            if(string.IsNullOrEmpty(Name)) {
                ErrorText = "Введите имя поискового набора.";
                return false;
            }

            if(_oldFilterNames.Contains(Name) || Name == _currentName) {
                ErrorText = "Поисковый набор с таким именем уже существует.";
                return false;
            }
            ErrorText = null;
            return true;
        }
    }
}
