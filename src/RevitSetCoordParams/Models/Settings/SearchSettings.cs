namespace RevitSetCoordParams.Models.Settings;

internal class SearchSettings {

    // средне-статистическое расстояние от удаленных элементов в моделях
    private const int _maxDiameterDefaultMm = 2000;
    // наиболее оптимальный шаг увеличения поисковой сферы
    private const int _stepDiameterDefaultMm = 200;

    public bool Search => true;
    public int MaxDiameterSearchSphereMm => _maxDiameterDefaultMm;
    public int StepDiameterSearchSphereMm => _stepDiameterDefaultMm;
}
