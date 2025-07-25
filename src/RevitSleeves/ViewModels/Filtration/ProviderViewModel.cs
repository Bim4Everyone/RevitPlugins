using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Utils;

namespace RevitSleeves.ViewModels.Filtration;
internal class ProviderViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    public ProviderViewModel(ILocalizationService localizationService, IFilterableValueProvider provider) {
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        Provider = provider
            ?? throw new System.ArgumentNullException(nameof(provider));
    }


    public string Name => Provider.Name;

    public string DisplayValue => Provider.DisplayValue;

    public IFilterableValueProvider Provider { get; }


    public string GetErrorText(string value) {
        return Provider.StorageType switch {
            StorageType.Integer => int.TryParse(value, out int _)
                ? string.Empty
                : string.Format(_localizationService.GetLocalizedString(
                    "SleevePlacementSettings.Validation.ParamValueMustBeInt"), Name),
            StorageType.Double => DoubleValueParser.TryParse(value, Provider.UnitType, out double _)
                ? string.Empty
                : string.Format(_localizationService.GetLocalizedString(
                    "SleevePlacementSettings.Validation.ParamValueMustBeDouble"), Name),
            StorageType.String => string.Empty,
            StorageType.ElementId => string.Empty,
            _ => string.Format(_localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.ParamHasNoDataType"), Name)
        };
    }
}
