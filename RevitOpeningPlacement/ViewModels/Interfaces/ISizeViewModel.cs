using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.Interfaces {
    internal interface ISizeViewModel {
        string Name { get; set; }
        double Value { get; set; }
        Size GetSize();
        string GetErrorText();
    }
}
