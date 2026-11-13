using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitDeclarations.ViewModels;
internal class WarningsViewModel : BaseViewModel {
    public bool IsWarning { get; set; }
    public ObservableCollection<WarningViewModel> Warnings { set;  get; }
}
