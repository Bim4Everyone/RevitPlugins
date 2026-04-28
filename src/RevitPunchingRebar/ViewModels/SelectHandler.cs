using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using RevitPunchingRebar.Models.SelectionFilters;

namespace RevitPunchingRebar.ViewModels;

internal enum SelectionMode {
    Single,
    MultiFromModel,
    MultiFromLink
}

public class SelectHandler : IExternalEventHandler {
    private readonly MainViewModel _viewModel;
    internal SelectionMode SelectionMode { get; set; }

    internal SelectHandler(MainViewModel viewModel) {
        _viewModel = viewModel;
    }

    public void Execute(UIApplication app) {
        try {
            if( SelectionMode == SelectionMode.Single) {
                Reference slabReference = app.ActiveUIDocument.Selection.PickObject(ObjectType.Element, new SlabFilter(), "Выберите плиту");
                _viewModel.SelectedSlabId = app.ActiveUIDocument.Document.GetElement(slabReference.ElementId).UniqueId;
            }

            if( SelectionMode == SelectionMode.MultiFromModel) {
                IList<Reference> pylonRefs = app.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, new PylonFromModelFilter(), "Выберите пилоны");
                List<string> ids = pylonRefs.Select(i => app.ActiveUIDocument.Document.GetElement(i).UniqueId).ToList();

                _viewModel.SelectedPylonsFromModel = ids;
            }

            if ( SelectionMode == SelectionMode.MultiFromLink) {
                IList<Reference> pylonRefs = app.ActiveUIDocument.Selection.PickObjects(ObjectType.LinkedElement, new PylonFromLinkFilter(app.ActiveUIDocument.Document), "Выберите пилоны");
                Dictionary<string, string> selectedPylons = new Dictionary<string, string>();

                foreach (Reference reference in pylonRefs) {
                    RevitLinkInstance link = app.ActiveUIDocument.Document.GetElement(reference.ElementId) as RevitLinkInstance;
                    Document linkedDoc = link.GetLinkDocument();
                    selectedPylons.Add(
                        linkedDoc.GetElement(reference.LinkedElementId).UniqueId,
                        linkedDoc.Title
                        );
                }

                _viewModel.SelectedPylonsFromLink = selectedPylons;
            }

        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            
        } finally {
            _viewModel.OnSelected();
        }
    }

    public string GetName() => "SelectHandler";
}
