using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface IFileProvider {

    FileProviderType Type { get; }

    Document Document { get; }

    ICollection<RevitElement> GetRevitElements();
}
