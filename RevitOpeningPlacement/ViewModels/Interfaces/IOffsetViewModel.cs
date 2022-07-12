using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.Interfaces {
    internal interface IOffsetViewModel {
        double From { get; set; }
        double To { get; set; }
        double Offset { get; set; }
        Offset GetOffset();
    }
}
