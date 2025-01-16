using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels {
    internal class UpdateLinksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _saveProperty;

        public UpdateLinksViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository;
            _localizationService = localizationService;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private void LoadView() {
        }

        private void AcceptView() {
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = _localizationService.GetLocalizedString("UpdateLinksWindow.HelloCheck");
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
