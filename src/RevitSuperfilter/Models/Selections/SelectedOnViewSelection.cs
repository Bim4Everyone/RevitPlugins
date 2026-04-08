using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.Revit;

using RevitSuperfilter.Comparators;

namespace RevitSuperfilter.Models.Selections;

internal sealed class SelectedOnViewSelection : BaseSelection, ISelectionElements {
    public SelectedOnViewSelection(UIApplication uiApplication)
        : base(uiApplication) {
        _uiApplication.SelectionChanged += UiApplicationOnSelectionChanged;
    }

    public override Selection Selection => Selection.SelectedOnViewSelection;

    public override IEnumerable<Element> GetElements() {
        return UIDocument.GetSelectedElements();
    }
    
    private void UiApplicationOnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        OnSelectionChange(new SelectionChangeEventArgs());
    }
    
    protected override void Dispose(bool disposing) {
        if(disposing) {
            _uiApplication.SelectionChanged -= UiApplicationOnSelectionChanged;
        }
        
        base.Dispose(disposing);
    }
}
