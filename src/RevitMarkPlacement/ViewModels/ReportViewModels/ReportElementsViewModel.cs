using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels.ReportViewModels;

internal class ReportElementsViewModel : BaseViewModel {
    private ObservableCollection<ReportElementViewModel> _reportElements;

    public ReportElementsViewModel() {
        ReportElements = [];
    }

    public ObservableCollection<ReportElementViewModel> ReportElements {
        get => _reportElements;
        set => RaiseAndSetIfChanged(ref _reportElements, value);
    }
}
