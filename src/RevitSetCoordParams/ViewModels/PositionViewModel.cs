using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;
internal class PositionViewModel {

    public string Name { get; set; }

    public IPositionProvider PositionProvider { get; set; }
}
