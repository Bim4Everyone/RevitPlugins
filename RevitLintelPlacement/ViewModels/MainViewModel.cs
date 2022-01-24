using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {

        private RuleCollectionViewModel _ruleCollectionViewModel;
        private LintelCollectionViewModel _lintelCollectionViewModel;

        public RuleCollectionViewModel RuleCollectionViewModel {
            get => _ruleCollectionViewModel;
            set => this.RaiseAndSetIfChanged(ref _ruleCollectionViewModel, value);
        }

        public LintelCollectionViewModel LintelCollectionViewModel {
            get => _lintelCollectionViewModel;
            set => this.RaiseAndSetIfChanged(ref _lintelCollectionViewModel, value);
        }
    }
}
