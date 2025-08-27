using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;
internal class NoticeListViewModel : BaseViewModel {
    private string _status;
    private string _description;

    public string Status {
        get => _status;
        set => RaiseAndSetIfChanged(ref _status, value);
    }
    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }

    public ObservableCollection<IWarningItemViewModel> ErrorElements { get; set; }
}
