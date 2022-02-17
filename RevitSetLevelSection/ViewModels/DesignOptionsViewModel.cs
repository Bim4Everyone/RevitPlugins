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
        private readonly List<FamilyInstance> _massElements;

        public DesignOptionsViewModel(DesignOption designOption, LinkInstanceRepository linkInstanceRepository) {
            if(designOption is null) {
                throw new ArgumentNullException(nameof(designOption));
            }

            if(linkInstanceRepository is null) {
                throw new ArgumentNullException(nameof(linkInstanceRepository));
            }

            DesignOption designOption1 = designOption;
            _massElements = linkInstanceRepository.GetMassElements(designOption1).ToList();

            Name = designOption1.Name;
        }

        public string Name { get; }

        public int CountMassElements => _massElements.Count;

        public IEnumerable<FamilyInstance> GetMassObjects() {
            return _massElements;
        }
    }
}
