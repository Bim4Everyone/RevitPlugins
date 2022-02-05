using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels {
    internal class LintelCornerParameter : BaseViewModel, ILintelParameterViewModel {
        private bool _isCornerChecked;

        public bool IsCornerChecked {
            get => _isCornerChecked;
            set => this.RaiseAndSetIfChanged(ref _isCornerChecked, value);
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            lintel.SetParamValue("Уголок", IsCornerChecked ? 0 : 1); //ToDo: параметр
        }
    }
}
