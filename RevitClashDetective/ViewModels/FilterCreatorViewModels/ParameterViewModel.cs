using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class ParameterViewModel : BaseViewModel {
        private IFilterableValueProvider _filterableValueProvider;
        private string _name;

        public IFilterableValueProvider FilterableValueProvider {
            get => _filterableValueProvider;
            set => this.RaiseAndSetIfChanged(ref _filterableValueProvider, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public ParameterViewModel(IFilterableValueProvider filterableValueProvider) {
            FilterableValueProvider = filterableValueProvider;
            Name = FilterableValueProvider.Name;
        }

        public IEnumerable<RuleEvaluator> GetEvaluators() {
            return FilterableValueProvider.GetRuleEvaluators();
        }

        public IEnumerable<object> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator) {
            return FilterableValueProvider.GetValues(categories, ruleEvaluator);
        }
    }
}
