using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    private string _errorText;

    public MainViewModel(
        ILocalizationService localizationService,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService) {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));

        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand AcceptViewCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void AcceptView() {
    }

    private bool CanAcceptView() {
        ErrorText = null;
        return true;
    }
}
