using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class PositionProviderCenter : IPositionProvider {

    private readonly RevitRepository _revitRepository;

    public PositionProviderCenter(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }
    public PositionProviderType Type => PositionProviderType.CenterPositionProvider;

    public ICollection<XYZ> GetPositionElement(Element element) {
        throw new System.NotImplementedException();
    }
}
