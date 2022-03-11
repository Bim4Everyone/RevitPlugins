using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class SaveAsViewModel : BaseViewModel {
        private string _rulesFileName;

        public string RulesFileName {
            get => _rulesFileName;
            set => this.RaiseAndSetIfChanged(ref _rulesFileName, value);
        }
    }
}
