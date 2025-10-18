using System.Collections.Generic;
using System.Linq;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Settings;

internal class ConfigSettings {
    // средне-статистическое максимальное расстояние от удаленных элементов в моделях
    private const double _maxDiameterSearchSphereMm = 2000;
    // наиболее оптимальный шаг увеличения поисковой сферы
    private const double _stepDiameterSearchSphereMm = 200;
    // обычно всегда используется поиск
    private const bool _search = true;
    // самое распространенное значения для запись координат
    private const string _typeModel = "Координаты СМР";

    public ProviderType ElementsProvider { get; set; }
    public ProviderType PositionProvider { get; set; }
    public SourceFile SourceFile { get; set; }
    public string TypeModel { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<RevitCategory> Categories { get; set; }
    public double MaxDiameterSearchSphereMm { get; set; }
    public double StepDiameterSearchSphereMm { get; set; }
    public bool Search { get; set; }

    public void ApplyDefaultValues() {
        ElementsProvider = ProviderType.AllElementsProvider;
        PositionProvider = ProviderType.CenterPositionProvider;
        SourceFile = new SourceFile {
            ProviderType = ProviderType.CoordFileProvider,
            SuorceFileUniqueId = null
        };
        TypeModel = _typeModel;
        ParamMaps = GetDefaultParams();
        Categories = GetDefaultCategories();
        MaxDiameterSearchSphereMm = _maxDiameterSearchSphereMm;
        StepDiameterSearchSphereMm = _stepDiameterSearchSphereMm;
        Search = _search;
    }

    private List<ParamMap> GetDefaultParams() {
        return new FixedParams().GetDefaultParams()
            .Select(param => new ParamMap {
                SourceParam = param.IsPair ? param.RevitParam : null,
                TargetParam = param.RevitParam,
                LocalizationKey = param.Key,
                IsChecked = true
            })
            .ToList();
    }

    private List<RevitCategory> GetDefaultCategories() {
        return new FixedCategories().GetDefaultBuiltInCategories()
            .Select(category => new RevitCategory {
                BuiltInCategory = category,
                IsChecked = true
            })
            .ToList();
    }
}
