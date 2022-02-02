using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels {
    internal class LintelTypeNameParameter : BaseViewModel, ILintelParameterViewModel {
        private string _name;

        public string Name { 
            get => _name; 
            set => this.RaiseAndSetIfChanged(ref _name, value); 
        }
        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            throw new NotImplementedException();
        }
    }


}
