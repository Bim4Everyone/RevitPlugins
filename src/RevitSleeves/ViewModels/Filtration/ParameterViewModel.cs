using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitSleeves.ViewModels.Filtration;
internal class ParameterViewModel : BaseViewModel, IEquatable<ParameterViewModel> {
    private readonly ILocalizationService _localizationService;

    public ParameterViewModel(ILocalizationService localizationService,
        IFilterableValueProvider filterableValueProvider) {

        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        ProviderViewModel = new ProviderViewModel(_localizationService, filterableValueProvider);
        Name = ProviderViewModel.DisplayValue;
    }

    public string Name { get; }

    public ProviderViewModel ProviderViewModel { get; }

    public IEnumerable<RuleEvaluator> GetEvaluators() {
        return ProviderViewModel.Provider.GetRuleEvaluators();
    }

    public IEnumerable<ParamValue> GetValues(Category category, RuleEvaluator ruleEvaluator) {
        return ProviderViewModel.Provider.GetValues([category], ruleEvaluator);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ParameterViewModel);
    }

    public override int GetHashCode() {
        int hashCode = -613960101;
        hashCode = hashCode * -1521134295 + EqualityComparer<IFilterableValueProvider>.Default.GetHashCode(
            ProviderViewModel.Provider);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        return hashCode;
    }

    public bool Equals(ParameterViewModel other) {
        return other != null
            && EqualityComparer<IFilterableValueProvider>.Default.Equals(
                ProviderViewModel.Provider, other.ProviderViewModel.Provider)
            && Name == other.Name;
    }
}
