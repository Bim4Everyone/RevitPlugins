using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;
internal abstract class NoticeViewModel : BaseViewModel {
    protected readonly ILocalizationService _localizationService;
    private readonly ObservableCollection<NoticeListViewModel> _noticeLists = [];
    private NoticeListViewModel _selectedList;
    private string _noticeInfoTitle;
    private string _noticeInfo;

    public NoticeViewModel(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public ObservableCollection<NoticeListViewModel> NoticeLists => _noticeLists;

    public string NoticeInfoTitle {
        get => _noticeInfoTitle;
        set => RaiseAndSetIfChanged(ref _noticeInfoTitle, value);
    }
    public string NoticeInfo {
        get => _noticeInfo;
        set => RaiseAndSetIfChanged(ref _noticeInfo, value);
    }

    public NoticeListViewModel SelectedList {
        get => _selectedList;
        set => RaiseAndSetIfChanged(ref _selectedList, value);
    }

    public void AddElements(NoticeListViewModel noticeList) {
        if(noticeList.ErrorElements.Any()) {
                _noticeLists.Add(noticeList);
        }

        SelectedList = _noticeLists.FirstOrDefault();
    }
}
