using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class SizeViewModel : BaseViewModel {
    private double _value;
    private string _displayName;
    private readonly string _sizeName;

    public SizeViewModel(Size size) {
        _sizeName = size.Name;
        DisplayName = size.Name;
        Value = size.Value;
    }

    public SizeViewModel() {

    }

    public string DisplayName {
        get => _displayName;
        set => RaiseAndSetIfChanged(ref _displayName, value);
    }
    public double Value {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }

    public string GetErrorText() {
        return Value < 0 ? $"значение параметра \"{DisplayName}\" должно быть неотрицательным." : null;
    }

    public Size GetSize() {
        return new Size() { Name = _sizeName, Value = Value };
    }
}
