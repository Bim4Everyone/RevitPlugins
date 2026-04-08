using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSuperfilter.Models.Selections;

namespace RevitSuperfilter.Models;

internal enum Selection {
    DBSelection,
    DBViewSelection,
    SelectedOnViewSelection
}

internal interface ISelection<out T> where T : Element {
    Selection Selection { get; }
    IEnumerable<T> GetElements();
}

internal interface ISelectionElements : ISelection<Element> {
    event EventHandler<SelectionChangeEventArgs> OnSelectionChanged;
}
