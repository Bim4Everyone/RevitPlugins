using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IElementsProvider {

    ElementsProviderType Type { get; }

    ICollection<RevitElement> GetRevitElements(IEnumerable<BuiltInCategory> categories);
}
