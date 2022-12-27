using System.Collections.ObjectModel;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.Interfaces {
    internal interface IOffsetViewModel {
        double From { get; set; }
        double To { get; set; }
        double Offset { get; set; }
        ObservableCollection<string> OpeningTypeNames { get; set; }
        string SelectedOpeningType { get; set; }
        string GetErrorText();
        string GetIntersectText(IOffsetViewModel offset);
        void Update(ITypeNamesProvider typeNamesProvider);
        Offset GetOffset();
    }
}
