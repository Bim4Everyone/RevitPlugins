using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Interfaces;

using RevitClashDetective.Models.Utils;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class ProviderViewModel : BaseViewModel {
        public ProviderViewModel(IFilterableValueProvider provider) {
            Provider = provider;
        }
        public string Name => Provider.Name;
        public string DisplayValue => Provider.DisplayValue;
        public RevitRepository RevitRepository { get; set; }
        public IFilterableValueProvider Provider { get; set; }


        public string GetErrorText(string value) {
            switch(Provider.StorageType) {
                default: {
                    throw new ArgumentOutOfRangeException(nameof(Provider.StorageType), $"У параметра {Name} не определен тип данных.");
                }
                case StorageType.Integer:
                return int.TryParse(value, out int intRes) ? null : $"Значение параметра \"{Name}\" должно быть целым числом.";
                case StorageType.Double: {
                    return DoubleValueParser.TryParse(value, Provider.UnitType, out double result) ? null : $"Значение параметра \"{Name}\" должно быть вещественным числом.";
                }
                case StorageType.String:
                return null;
                case StorageType.ElementId:
                return null;
            }
        }
    }
}
