using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class UnplacedClashModel {
        public string Message { get; set; }
        public ClashModel Clash { get; set; }
    }
}
