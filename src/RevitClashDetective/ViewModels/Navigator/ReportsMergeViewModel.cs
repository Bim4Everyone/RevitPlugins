using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportsMergeViewModel : BaseViewModel {
    private string _errorText;

    public ReportsMergeViewModel() {
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
}
