using System;
using System.Collections.Generic;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class CurrentViewElementsProvider : IElementsProvider {
    public string Name => throw new NotImplementedException();

    public ICollection<RevitElement> GetElementModels() {
        throw new NotImplementedException();
    }
}
