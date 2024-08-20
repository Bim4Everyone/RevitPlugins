using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGenLookupTables.ViewModels {
    internal class SelectFamilyParamsViewModel {
        public FamilyParamViewModel SelectedFamilyParam { get; set; }
        public ObservableCollection<FamilyParamViewModel> FamilyParams { get; set; }
        public ObservableCollection<FamilyParamViewModel> SelectedFamilyParams { get; set; } = new ObservableCollection<FamilyParamViewModel>();
    }
}
