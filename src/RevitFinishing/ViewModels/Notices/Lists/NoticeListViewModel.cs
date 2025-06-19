using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;
internal class NoticeListViewModel : BaseViewModel {
    public string Status { get; set; }
    public string Description { get; set; }

    public ObservableCollection<NoticeElementViewModel> ErrorElements { get; set; }
}
