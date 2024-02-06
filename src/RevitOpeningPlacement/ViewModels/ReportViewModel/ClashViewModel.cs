using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.ViewModels.ReportViewModel {
    internal class ClashViewModel : BaseViewModel {
        public ClashViewModel(ClashModel clash) {
            Clash = clash;
        }
        public ClashViewModel(UnplacedClashModel unplacedClash) {
            Clash = unplacedClash.Clash;
            Message = unplacedClash.Message;
        }
        public ClashModel Clash { get; set; }
        public string Message { get; set; }
    }
}
