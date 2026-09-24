using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;

namespace RevitEnumerateBySpline.ViewModels;

internal class SpatialModelViewModel : BaseViewModel {
    private string _name = string.Empty;
    private bool _isChecked;
    private SpatialModel? _spatialModel;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
    
    public SpatialModel? SpatialModel {
        get => _spatialModel;
        set => RaiseAndSetIfChanged(ref _spatialModel, value);
    }
}
