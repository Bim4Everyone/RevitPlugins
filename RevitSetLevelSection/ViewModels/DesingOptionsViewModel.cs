using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class DesingOptionsViewModel : BaseViewModel {
        private readonly DesignOption _designOption;
        private readonly RevitRepository _revitRepository;

        public DesingOptionsViewModel(DesignOption designOption, RevitRepository revitRepository) {
            if(designOption is null) {
                throw new ArgumentNullException(nameof(designOption));
            }

            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _designOption = designOption;
            _revitRepository = revitRepository;
        }

        public string Name => _designOption.Name;

        public IEnumerable<FamilyInstance> GetMassObjects() {
            return _revitRepository.GetMassElements(_designOption);
        }
    }
}
