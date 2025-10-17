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

    public ElementsProviderType ElementsProvider { get; set; }
    public FileProviderType FileProvider { get; set; }
    public string TypeModel { get; set; }
    public PositionProviderType PositionProvider { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<RevitCategory> Categories { get; set; }
    public double MaxDiameterSearchSphereMm { get; set; }
    public double StepDiameterSearchSphereMm { get; set; }
    public bool Search { get; set; }


    public void ApplyDefaultValues() {
        ElementsProvider = ElementsProviderType.AllElements;
        FileProvider = FileProviderType.CoordFile;
        TypeModel = _typeModel;
        PositionProvider = PositionProviderType.Center;
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
