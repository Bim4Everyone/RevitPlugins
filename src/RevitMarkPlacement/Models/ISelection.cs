using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models;

internal interface ISelection<out T> where T : Element {
    IEnumerable<T> GetElements();
}
