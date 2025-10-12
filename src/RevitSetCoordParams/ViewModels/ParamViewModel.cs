using dosymep.WPF.ViewModels;

namespace RevitSetCoordParams.ViewModels;

internal class ParamViewModel : BaseViewModel {
    public string Header { get; set; }
    public string Description { get; set; }

    public string CommonParamHeader { get; set; }
    public string CommonParam { get; set; }

    public string ElementParamHeader { get; set; }
    public string ElementParam { get; set; }

    public string SwitchOnContent { get; set; } = "Использовать";
    public string SwitchOffContent { get; set; } = "Не использовать";

    public bool IsChecked = true;

}
