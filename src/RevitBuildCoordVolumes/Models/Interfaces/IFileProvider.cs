using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IFileProvider {
    /// <summary>
    /// Документ (связанный или текущий)
    /// </summary>
    Document Document { get; }
    /// <summary>
    /// Метод получения элементов модели по строке (значению параметра)
    /// </summary>
    ICollection<SlabElement> GetRevitElements(string typeModel);
}
