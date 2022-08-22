using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.ViewModels.ReportViewModel {
    internal class ClashViewModel : BaseViewModel {
        public ClashViewModel(ClashModel clash) {
            Clash = clash;
        }
        public ClashModel Clash { get; set; }
    }
}
