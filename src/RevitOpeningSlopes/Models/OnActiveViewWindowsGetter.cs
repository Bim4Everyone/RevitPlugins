using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class OnActiveViewWindowsGetter : IWindowsGetter {
        private readonly RevitRepository _revitRepository;

        public OnActiveViewWindowsGetter(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        }

        public string Name => "По окнам на активном виде";
        public ICollection<FamilyInstance> Openings => GetOpenings();
        public ICollection<FamilyInstance> GetOpenings() {
            return _revitRepository.GetWindowsOnActiveView();
        }
    }
}
