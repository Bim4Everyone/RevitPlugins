using System.Collections.Generic;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IElementsProvider {
    string Name { get; }
    ICollection<RevitElement> GetElementModels();
}
