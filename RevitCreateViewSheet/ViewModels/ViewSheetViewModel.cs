using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewSheetViewModel : BaseViewModel {
        private string _name;
        private TitleBlockViewModel _titleBlock;

        public ViewSheetViewModel() {
            Name = "Без имени";
        }

        public FamilySymbol FamilySymbol {
            get => _titleBlock.FamilySymbol;
        }

        public string Name {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public TitleBlockViewModel TitleBlock {
            get => _titleBlock;
            set => RaiseAndSetIfChanged(ref _titleBlock, value);
        }

        public override string ToString() {
            return Name;
        }
    }
}
