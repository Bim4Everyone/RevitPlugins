using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevExpress.Mvvm;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class EvaluatorViewModel : BaseViewModel {
        private string _name;

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
    }
}
