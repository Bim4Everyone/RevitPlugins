using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {

        private RuleCollectionViewModel _rules;
        private LintelCollectionViewModel _lintels;

        public MainViewModel() {

        }

        public MainViewModel(RuleConfig ruleConfig) {
            Rules = new RuleCollectionViewModel(ruleConfig);
            Lintels = new LintelCollectionViewModel();
        }

        public RuleCollectionViewModel Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }

        public LintelCollectionViewModel Lintels {
            get => _lintels;
            set => this.RaiseAndSetIfChanged(ref _lintels, value);
        }
    }
}
