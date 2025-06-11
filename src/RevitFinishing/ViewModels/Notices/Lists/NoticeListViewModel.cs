using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels
{
    internal class NoticeListViewModel : BaseViewModel {
        public string Status { get; set; }
        public string Description { get; set; }

        public ObservableCollection<NoticeElementViewModel> ErrorElements { get; set; }
    }
}
