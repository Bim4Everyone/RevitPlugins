using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitMarkAllDocuments.ViewModels
{
    internal class WarningViewModel : BaseViewModel {
        public string FullName { get; set; }
        public string Description { get; set; }
        public ObservableCollection<WarningElementViewModel> Elements { get; set; } = [];
    }
}
