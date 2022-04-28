using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class ParameterViewModel : BaseViewModel {
        private string _name;

        public ParameterViewModel(string name) {
            Name = name;
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public override bool Equals(object obj) {
            var smth = obj.GetType();
            return obj is ParameterViewModel model &&
                   Name == model.Name;
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
