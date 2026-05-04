using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels;

internal class WarningsViewModel : BaseViewModel {
    public ObservableCollection<WarningViewModel> Warnings { set; get; } = [];
}
