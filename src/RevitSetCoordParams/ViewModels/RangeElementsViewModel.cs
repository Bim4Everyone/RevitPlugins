using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;
internal class RangeElementsViewModel {

    public string Name { get; set; }

    public IElementsProvider ElementsProvider { get; set; }

}
