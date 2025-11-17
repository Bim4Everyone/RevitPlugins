using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels;
internal class WarningsViewModel : BaseViewModel {
    public ObservableCollection<WarningViewModel> Warnings { get; set; }
}
