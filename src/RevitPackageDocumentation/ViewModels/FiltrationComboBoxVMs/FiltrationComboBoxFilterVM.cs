using dosymep.WPF.ViewModels;

namespace RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
internal class FiltrationComboBoxFilterVM : BaseViewModel {
    private string _value = string.Empty;

    public string Value {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }
}
