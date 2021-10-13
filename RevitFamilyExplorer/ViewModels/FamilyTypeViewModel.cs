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
        public FamilyTypeViewModel(RevitRepository revitRepository, string familyTypeName) {
            _revitRepository = revitRepository;
            Name = familyTypeName;
        }

        public string Name { get; }
    }
}
