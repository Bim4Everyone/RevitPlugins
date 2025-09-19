using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels;
internal class PhaseViewModel : ElementViewModel<Phase> {
    public PhaseViewModel(Phase phase, RevitRepository revitRepository)
        : base(phase, revitRepository) {
    }
}
