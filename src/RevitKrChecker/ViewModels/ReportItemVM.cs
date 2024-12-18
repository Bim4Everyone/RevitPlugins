// Ignore Spelling: Tooltip Сheck

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitKrChecker.Models;

namespace RevitKrChecker.ViewModels {
    internal class ReportItemVM : BaseViewModel {
        public ReportItemVM(CheckInfo checkInfo) {
            Elem = checkInfo.Elem;
            TargetParamName = checkInfo.TargetParamName;
            СheckName = checkInfo.СheckName;
            ElementErrorTooltip = checkInfo.ElementErrorTooltip;
        }

        public Element Elem { get; }
        public string TargetParamName { get; }
        public string СheckName { get; }
        public string ElementErrorTooltip { get; }
    }
}
