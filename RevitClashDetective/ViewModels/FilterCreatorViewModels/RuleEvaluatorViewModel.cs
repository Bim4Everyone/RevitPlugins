using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class RuleEvaluatorViewModel : BaseViewModel {
        private RuleEvaluator _ruleEvaluator;
        private string _name;

        public RuleEvaluatorViewModel(RuleEvaluator ruleEvaluator) {
            RuleEvaluator = ruleEvaluator;
            Name = RuleEvaluator.Message;
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public RuleEvaluator RuleEvaluator {
            get => _ruleEvaluator;
            set => this.RaiseAndSetIfChanged(ref _ruleEvaluator, value);
        }

    }
}
