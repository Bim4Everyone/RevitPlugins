using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IPositionProvider {

    ProviderType Type { get; }

    ICollection<XYZ> GetPositionElement(Element element);
}
