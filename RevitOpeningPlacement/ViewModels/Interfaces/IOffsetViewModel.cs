using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.Interfaces {
    internal interface IOffsetViewModel {
        double From { get; set; }
        double To { get; set; }
        double Offset { get; set; }
        string GetErrorText();
        string GetIntersectText(IOffsetViewModel offset);
        Offset GetOffset();
    }
}
