using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IPositionProvider {

    PositionProviderType Type { get; }

    XYZ GetPositionElement(RevitElement revitElement);
}
