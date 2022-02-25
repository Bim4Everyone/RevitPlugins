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

            _massElements = linkInstanceRepository.GetMassElements(designOption).ToList();

            Id = designOption.Id.IntegerValue;
            Name = designOption.Name;
            Transform = linkInstanceRepository.Transform;
        }

        public int Id { get; }
        public string Name { get; }
        public Transform Transform { get; }

        public int CountMassElements => _massElements.Count;

        public IEnumerable<FamilyInstance> GetMassObjects() {
            return _massElements;
        }
    }
}
