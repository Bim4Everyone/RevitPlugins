using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;

namespace RevitEnumerateBySpline.ViewModels;

/// <summary>Представление выбранной кривой.</summary>
internal class CurveModelViewModel : BaseViewModel {
    private int _number;
    private string _markerColor = "#2563EB";
    private string _name = string.Empty;

    /// <summary>Порядковый номер линии в списке и на метке предпросмотра.</summary>
    public int Number {
        get => _number;
        set => RaiseAndSetIfChanged(ref _number, value);
    }
    
    /// <summary>Цвет номерной метки в интерфейсе и на виде Revit, в формате HTML.</summary>
    public string MarkerColor {
        get => _markerColor;
        set => RaiseAndSetIfChanged(ref _markerColor, value);
    }
    
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    
    public CurveModel? CurveModel{ get; set; }
}
