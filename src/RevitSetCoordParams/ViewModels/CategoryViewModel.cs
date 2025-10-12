using dosymep.WPF.ViewModels;

namespace RevitSetCoordParams.ViewModels;

internal class CategoryViewModel : BaseViewModel {
    public string CategoryName { get; set; }
    public bool IsChecked { get; set; }
    public bool HasWarning { get; set; }
    public string WarningDescription { get; set; }
}
