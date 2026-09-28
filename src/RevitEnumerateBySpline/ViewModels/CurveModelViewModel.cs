using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;

namespace RevitEnumerateBySpline.ViewModels;

internal class CurveModelViewModel : BaseViewModel {
    private string _name = string.Empty;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    
    public CurveModel? CurveModel{ get; set; }
}
