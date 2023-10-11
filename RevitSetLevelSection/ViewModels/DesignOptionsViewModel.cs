using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.ViewModels {
    internal class DesignOptionsViewModel : BaseViewModel {
        private readonly IDesignOption _designOption;
        private readonly List<FamilyInstance> _massElements;

        public DesignOptionsViewModel(IDesignOption designOption, IMassRepository massRepository) {
            if(designOption is null) {
                throw new ArgumentNullException(nameof(designOption));
            }

            if(massRepository is null) {
                throw new ArgumentNullException(nameof(massRepository));
            }

            _designOption = designOption;
            _massElements = massRepository.GetElements(designOption).ToList();

            Id = designOption.Id;
            Name = designOption.Name;
            Transform = massRepository.Transform;
            HasMassIntersect = massRepository.HasIntersects(designOption);
        }

        public ElementId Id { get; }
        public string Name { get; }
        public Transform Transform { get; }
        public bool HasMassIntersect { get; }

        public IDesignOption DesignOption => _designOption;

        public int CountMassElements => _massElements.Count;

        public IEnumerable<FamilyInstance> GetMassObjects() {
            return _massElements;
        }
    }
}
