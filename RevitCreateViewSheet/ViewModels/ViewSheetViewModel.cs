using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewSheetViewModel : BaseViewModel {
        private string _name;
        private FamilyInstanceViewModel _familyInstance;

        public ViewSheetViewModel() {
            Name = "Без имени";
        }

        public string Name {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public FamilyInstanceViewModel FamilyInstance {
            get => _familyInstance;
            set => RaiseAndSetIfChanged(ref _familyInstance, value);
        }
    }
}
