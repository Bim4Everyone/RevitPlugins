using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Services;
using RevitCheckingLevels.Views;

namespace RevitCheckingLevels.ViewModels {
    internal class CheckingLinkLevelsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private LinkTypeViewModel _linkTypeViewModel;

        public CheckingLinkLevelsViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            ViewCommand = new RelayCommand(Execute);
            ViewLoadCommand = new RelayCommand(Load);
        }

        public ICommand ViewCommand { get; }
        public ICommand ViewLoadCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private void Execute(object p) {

        }

        private void Load(object p) {

        }
    }
}
