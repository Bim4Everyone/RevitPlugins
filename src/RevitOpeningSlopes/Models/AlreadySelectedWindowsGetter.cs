using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class AlreadySelectedWindowsGetter : IWindowsGetter {
        private readonly RevitRepository _revitRepository;

        public AlreadySelectedWindowsGetter(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        }
        public string Name => $"По предварительно выбранным окнам ({this.GetOpenings().Count})";

        public ICollection<FamilyInstance> GetOpenings() {
            return _revitRepository.GetSelectedWindows();
        }
    }
}
