using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class ParameterViewModel : BaseViewModel, IEquatable<ParameterViewModel> {
        private ProviderViewModel _filterableValueProvider;
        private string _name;

        public ProviderViewModel FilterableValueProvider {
            get => _filterableValueProvider;
            set => this.RaiseAndSetIfChanged(ref _filterableValueProvider, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public ParameterViewModel(IFilterableValueProvider filterableValueProvider) {
            FilterableValueProvider = new ProviderViewModel(filterableValueProvider);
            Name = FilterableValueProvider.DisplayValue;
        }

        public IEnumerable<RuleEvaluator> GetEvaluators() {
            return FilterableValueProvider.Provider.GetRuleEvaluators();
        }

        public IEnumerable<ParamValue> GetValues(Category[] categories, RuleEvaluator ruleEvaluator) {
            return FilterableValueProvider.Provider.GetValues(categories, ruleEvaluator);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParameterViewModel);
        }

        public override int GetHashCode() {
            int hashCode = -613960101;
            hashCode = hashCode * -1521134295 + EqualityComparer<IFilterableValueProvider>.Default.GetHashCode(FilterableValueProvider.Provider);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public bool Equals(ParameterViewModel other) {
            return other != null && EqualityComparer<IFilterableValueProvider>.Default.Equals(FilterableValueProvider.Provider, other.FilterableValueProvider.Provider) &&
                   Name == other.Name;
        }
    }
}
