using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models.Selections;

internal class SelectionChangeEventArgs : EventArgs {
    public SelectionChangeEventArgs() {
        AddedElementIds = [];
        RemovedElementIds = [];
        ModifiedElementIds = [];
    }

    public SelectionChangeEventArgs(
        IEnumerable<ElementId> addedElementIds,
        IEnumerable<ElementId> removedElementIds,
        IEnumerable<ElementId> modifiedElementIds) {
        AddedElementIds = addedElementIds.ToList();
        RemovedElementIds = removedElementIds.ToList();
        ModifiedElementIds = modifiedElementIds.ToList();
    }

    public bool IsEmpty =>
        AddedElementIds.Count == 0
        && RemovedElementIds.Count == 0
        && ModifiedElementIds.Count == 0;

    public IReadOnlyCollection<ElementId> AddedElementIds { get; }
    public IReadOnlyCollection<ElementId> RemovedElementIds { get; }
    public IReadOnlyCollection<ElementId> ModifiedElementIds { get; }
}
