using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class NameInputViewModel : BaseViewModel {
    public event Action AcceptRequested;

    private readonly ILocalizationService _localization;
    private Func<string, bool> _validate;
    private string _title;
    private string _name;
    private string _errorText;
    private bool _isValid;

    public NameInputViewModel(ILocalizationService localization, ISettingsCommandFactory commandFactory) {
        _localization = localization;
        AcceptCommand = commandFactory.Create(() => AcceptRequested?.Invoke(), () => _isValid);
    }

    public ICommand AcceptCommand { get; }

    public string Title {
        get => _title;
        private set => RaiseAndSetIfChanged(ref _title, value);
    }

    public string Name {
        get => _name;
        set {
            RaiseAndSetIfChanged(ref _name, value);
            _isValid = _validate?.Invoke(value?.Trim() ?? string.Empty) == true;
            ErrorText = _isValid ? null : _localization.GetLocalizedString("Dialog.InvalidName");
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public void Initialize(string title, string initialName, Func<string, bool> validate) {
        _validate = validate ?? throw new ArgumentNullException(nameof(validate));
        Title = title;
        Name = initialName ?? string.Empty;
    }
}
