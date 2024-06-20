using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class ReportVMTEST : BaseViewModel {

        public ReportVMTEST() {

        }
        public List<ReportItemSimple> ReportItems { get; set; } = new List<ReportItemSimple>();

    }
}
