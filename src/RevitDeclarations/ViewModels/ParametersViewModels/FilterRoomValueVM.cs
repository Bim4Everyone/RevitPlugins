using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels {
    internal class FilterRoomValueVM : BaseViewModel {
        private readonly ParametersViewModel _paramViewModel;
        private readonly string _value;

        public FilterRoomValueVM(ParametersViewModel paramViewModel, string value) {
            _paramViewModel = paramViewModel;
            _value = value;

            RemoveFilterCommand = new RelayCommand(RemoveFilter);
        }

        public ICommand RemoveFilterCommand { get; }

        public string Value => _value;

        public void RemoveFilter(object o) {
            _paramViewModel.RemoveFilter(this);
        }
    }
}
