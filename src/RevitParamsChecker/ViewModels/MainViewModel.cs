using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;

namespace RevitParamsChecker.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;

    public MainViewModel(
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
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
    }

    private void AcceptView() {
    }

    private bool CanAcceptView() {
        ErrorText = null;
        return true;
    }
}
