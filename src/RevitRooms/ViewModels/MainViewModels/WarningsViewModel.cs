using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels;
internal class WarningsViewModel : BaseViewModel {
    public string Description { get; set; }
    public ObservableCollection<WarningViewModel> Warnings { get; set; }
}
