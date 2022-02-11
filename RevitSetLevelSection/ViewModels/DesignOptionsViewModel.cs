using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class DesignOptionsViewModel : BaseViewModel {
        private readonly DesignOption _designOption;
        private readonly LinkInstanceRepository _linkInstanceRepository;

        public DesignOptionsViewModel(DesignOption designOption, LinkInstanceRepository linkInstanceRepository) {
            if(designOption is null) {
                throw new ArgumentNullException(nameof(designOption));
            }

            if(linkInstanceRepository is null) {
                throw new ArgumentNullException(nameof(linkInstanceRepository));
            }

            _designOption = designOption;
            _linkInstanceRepository = linkInstanceRepository;
        }

        public string Name => _designOption.Name;

        public IEnumerable<FamilyInstance> GetMassObjects() {
            return _linkInstanceRepository.GetMassElements(_designOption);
        }
    }
}
