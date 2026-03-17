using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitVolumeModifier.Models;
public class SelectionMonitor {
    private readonly UIApplication _uiApp;
    private ICollection<ElementId> _lastSelection = [];

    public event Action<ICollection<ElementId>> SelectionChanged;

    public SelectionMonitor(UIApplication uiApp) {
        _uiApp = uiApp;
        _uiApp.Idling += OnIdling;
    }

    private void OnIdling(object sender, IdlingEventArgs e) {
        var uidoc = _uiApp.ActiveUIDocument;
        if(uidoc == null) {
            return;
        }

        var current = uidoc.Selection.GetElementIds();

        if(!IsEqual(current, _lastSelection)) {
            _lastSelection = current.ToList();
            SelectionChanged?.Invoke(_lastSelection);
        }
    }

    private bool IsEqual(ICollection<ElementId> a, ICollection<ElementId> b) {
        return a.Count == b.Count && !a.Except(b).Any();
    }

    public void Dispose() {
        _uiApp.Idling -= OnIdling;
    }
}
