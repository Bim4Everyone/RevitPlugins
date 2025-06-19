using dosymep.WPF;

namespace RevitHideWorkset.Models;
internal class WorksetElement : ObservableObject {

    private bool _isOpen;
    private bool _isChanged;

    public string Name { get; set; }

    public bool IsChanged {
        get => _isChanged;
        set => RaiseAndSetIfChanged(ref _isChanged, value);
    }

    public bool IsOpen {
        get => _isOpen;
        set => RaiseAndSetIfChanged(ref _isOpen, value);
    }
}
