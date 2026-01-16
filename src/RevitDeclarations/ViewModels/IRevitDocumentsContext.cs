using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDeclarations.ViewModels;

internal interface IRevitDocumentsContext {
    IList<RevitDocumentViewModel> RevitDocuments { get; }
    IReadOnlyList<Phase> Phases { get; }
    Phase SelectedPhase { get; set; }
}
