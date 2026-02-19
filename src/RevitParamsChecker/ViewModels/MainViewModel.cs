using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;

    private string _errorText;

    public MainViewModel(
        ILocalizationService localization,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        IProgressDialogFactory progressDialogFactory) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        ProgressDialogFactory = progressDialogFactory ?? throw new ArgumentNullException(nameof(progressDialogFactory));

        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand AcceptViewCommand { get; }
    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IProgressDialogFactory ProgressDialogFactory { get; }

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
