using System.Collections;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkInstanceViewModel : BaseViewModel {
        private readonly RevitLinkInstance _revitLinkInstance;
        private readonly RevitRepository _revitRepository;

        public LinkInstanceViewModel(RevitLinkInstance revitLinkInstance, RevitRepository revitRepository) {
            _revitLinkInstance = revitLinkInstance;
            _revitRepository = revitRepository;
        }

        public string Name => _revitLinkInstance.Name;
    }
}