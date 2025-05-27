using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitFinishing.Models;

namespace RevitFinishing.ViewModels
{
    internal class ErrorsListViewModel : BaseViewModel {
        public ErrorsListViewModel(string status) {
            Status = status;
        }
        public string Status { get; }
        public string Description { get; set; }

        public ObservableCollection<ErrorElement> ErrorElements { get; set; }
    }
}
