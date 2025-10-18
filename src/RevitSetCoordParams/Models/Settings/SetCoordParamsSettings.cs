using System.Collections.Generic;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Settings;
internal class SetCoordParamsSettings {

    private readonly RevitRepository _revitRepository;
    private readonly ProvidersFactory _providersFactory;

    public SetCoordParamsSettings(RevitRepository revitRepository, ConfigSettings configSettings) {
        _revitRepository = revitRepository;
        ConfigSettings = configSettings;
        _providersFactory = new ProvidersFactory();
    }

    public ConfigSettings ConfigSettings { get; private set; }

    public IElementsProvider ElementsProvider { get; set; }
    public IPositionProvider PositionProvider { get; set; }
    public SourceFile SourceFile { get; set; }
    public string TypeModel { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<RevitCategory> RevitCategories { get; set; }
    public double MaxDiameterSearchSphereMm { get; set; }
    public double StepDiameterSearchSphereMm { get; set; }
    public bool Search { get; set; }

    public void LoadConfigSettings() {
        ParamMaps = ConfigSettings.ParamMaps;
        RevitCategories = ConfigSettings.Categories;
        ElementsProvider = _providersFactory
            .GetElementsProvider(_revitRepository, ConfigSettings.ElementsProvider, RevitCategories);
        PositionProvider = _providersFactory
            .GetPositionProvider(_revitRepository, ConfigSettings.PositionProvider);
        SourceFile = ConfigSettings.SourceFile;
        MaxDiameterSearchSphereMm = ConfigSettings.MaxDiameterSearchSphereMm;
        StepDiameterSearchSphereMm = ConfigSettings.StepDiameterSearchSphereMm;
        Search = ConfigSettings.Search;
    }

    public void UpdateConfigSettings() {
        ConfigSettings.ParamMaps = ParamMaps;
        ConfigSettings.Categories = RevitCategories;
        ConfigSettings.ElementsProvider = ElementsProvider.Type;
        ConfigSettings.PositionProvider = PositionProvider.Type;
        ConfigSettings.SourceFile = SourceFile;
        ConfigSettings.TypeModel = TypeModel;
        ConfigSettings.MaxDiameterSearchSphereMm = MaxDiameterSearchSphereMm;
        ConfigSettings.StepDiameterSearchSphereMm = StepDiameterSearchSphereMm;
        ConfigSettings.Search = Search;
    }
}

