using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkInstanceViewModel : BaseViewModel {
        private readonly RevitLinkType _revitLinkType;
        private readonly RevitLinkInstance _revitLinkInstance;
        private readonly LinkInstanceRepository _linkInstanceRepository;

        public LinkInstanceViewModel(RevitLinkType revitLinkType, RevitLinkInstance revitLinkInstance) {
            _revitLinkType = revitLinkType;
            _revitLinkInstance = revitLinkInstance;
            _linkInstanceRepository = new LinkInstanceRepository(revitLinkType.Document.Application, revitLinkInstance.GetLinkDocument());
        }

        public string Name => _revitLinkType.Name;

        public IEnumerable<DesignOptionsViewModel> GetDesignOptions() {
            return _linkInstanceRepository.GetDesignOptions()
                .Select(item => new DesignOptionsViewModel(item, _linkInstanceRepository));;
        }
    }
}