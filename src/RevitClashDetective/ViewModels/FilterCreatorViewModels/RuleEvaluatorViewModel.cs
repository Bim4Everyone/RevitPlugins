using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class RuleEvaluatorViewModel : BaseViewModel, IEquatable<RuleEvaluatorViewModel> {
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

        public override bool Equals(object obj) {
            return Equals(obj as RuleEvaluatorViewModel);
        }

        public bool Equals(RuleEvaluatorViewModel other) {
            return other != null && Name == other.Name
                   && RuleEvaluator.Evaluator == other.RuleEvaluator.Evaluator;
        }

        public override int GetHashCode() {
            int hashCode = 1953519430;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<RuleEvaluators>.Default.GetHashCode(RuleEvaluator.Evaluator);
            return hashCode;
        }
    }
}
