using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class ParameterViewModel : BaseViewModel, IEquatable<ParameterViewModel> {
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

        public IEnumerable<ParamValueViewModel> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator) {
            return FilterableValueProvider.GetValues(categories, ruleEvaluator);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParameterViewModel);
        }

        public override int GetHashCode() {
            int hashCode = -613960101;
            hashCode = hashCode * -1521134295 + EqualityComparer<IFilterableValueProvider>.Default.GetHashCode(FilterableValueProvider);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public bool Equals(ParameterViewModel other) {
            return other != null && EqualityComparer<IFilterableValueProvider>.Default.Equals(FilterableValueProvider, other.FilterableValueProvider) &&
                   Name == other.Name;
        }
    }
}
