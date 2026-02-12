using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;

internal class RevitDocumentsContext : BaseViewModel, IRevitDocumentsContext {
    public IList<RevitDocumentViewModel> RevitDocuments { get; }
    public IReadOnlyList<Phase> Phases { get; }

    private Phase _selectedPhase;
    public Phase SelectedPhase {
        get => _selectedPhase;
        set => RaiseAndSetIfChanged(ref _selectedPhase, value);
    }

    public RevitDocumentsContext(RevitRepository revitRepository,
                                 DeclarationSettings settings) {
        Phases = revitRepository.GetPhases();
        SelectedPhase = Phases.Last();

        RevitDocuments = revitRepository
            .GetLinks()
            .Select(x => new RevitDocumentViewModel(x, settings))
            .Where(x => x.HasRooms())
            .OrderBy(x => x.Name)
            .ToList();

        var current =
            new RevitDocumentViewModel(revitRepository.Document, settings);

        if(current.HasRooms()) {
            RevitDocuments.Insert(0, current);
        }
    }
}
