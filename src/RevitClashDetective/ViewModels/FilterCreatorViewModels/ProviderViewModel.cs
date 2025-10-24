using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Utils;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class ProviderViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;

    public ProviderViewModel(ILocalizationService localization, IFilterableValueProvider provider) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
    public string Name => Provider.Name;
    public string DisplayValue => Provider.DisplayValue;
    public RevitRepository RevitRepository { get; set; }
    public IFilterableValueProvider Provider { get; set; }


    public string GetErrorText(string value) {
        switch(Provider.StorageType) {
            case StorageType.Integer:
                return int.TryParse(value, out int intRes)
                    ? null
                    : _localization.GetLocalizedString("Parameter.CannotParseInt", Name);
            case StorageType.Double: {
                return DoubleValueParser.TryParse(value, Provider.UnitType, out double result)
                    ? null
                    : _localization.GetLocalizedString("Parameter.CannotParseDouble", Name);
            }
            case StorageType.String:
                return null;
            case StorageType.ElementId:
                return null;
            default: {
                throw new ArgumentOutOfRangeException(nameof(Provider.StorageType),
                    _localization.GetLocalizedString("Parameter.InvalidStorageType", Name));
            }
        }
    }
}
