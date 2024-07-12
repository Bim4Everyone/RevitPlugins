using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class ManuallySelectedWindowsGetter : IWindowsGetter {
        private readonly RevitRepository _revitRepository;

        public ManuallySelectedWindowsGetter(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        }

        public string Name => "По выбранным вручную окнам";
        public ICollection<FamilyInstance> Openings => GetOpenings();
        public ICollection<FamilyInstance> GetOpenings() {
            return _revitRepository.SelectWindowsOnView();
        }
    }
}
