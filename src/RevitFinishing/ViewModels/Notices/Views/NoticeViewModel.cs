using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;
internal class NoticeViewModel : BaseViewModel {
    private protected readonly ILocalizationService _localizationService;
        private readonly ObservableCollection<NoticeListViewModel> _noticeLists = [];
    private NoticeListViewModel _selectedList;
    private string _noticeInfo;

    public NoticeViewModel(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

        public ObservableCollection<NoticeListViewModel> NoticeLists => _noticeLists;
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
    }
}
