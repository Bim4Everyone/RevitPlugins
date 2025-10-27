using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface IFileProvider {
    Document Document { get; }
    ICollection<RevitElement> GetRevitElements(string typeModel);
}
