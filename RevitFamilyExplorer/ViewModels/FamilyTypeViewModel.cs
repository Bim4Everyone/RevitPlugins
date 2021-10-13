using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyTypeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyType _familyType;

        public FamilyTypeViewModel(RevitRepository revitRepository, FamilyType familyType) {
            _revitRepository = revitRepository;
            _familyType = familyType;
        }

        public string Name {
            get { return _familyType.Name; }
        }
    }
}
