using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

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
    public IFileProvider FileProvider { get; set; }
    public ISphereProvider SphereProvider { get; set; }
    public List<string> TypeModels { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<BuiltInCategory> Categories { get; set; }
    public double MaxDiameterSearchSphereMm { get; set; }
    public double StepDiameterSearchSphereMm { get; set; }
    public bool Search { get; set; }

    public void LoadConfigSettings() {
        ParamMaps = ConfigSettings.ParamMaps;
        Categories = ConfigSettings.Categories;
        ElementsProvider = _providersFactory.GetElementsProvider(_revitRepository, ConfigSettings.ElementsProvider);
        PositionProvider = _providersFactory.GetPositionProvider(_revitRepository, ConfigSettings.PositionProvider);
        FileProvider = GetFileProvider(ConfigSettings.SourceFile);
        TypeModels = ConfigSettings.TypeModels;
        MaxDiameterSearchSphereMm = ConfigSettings.MaxDiameterSearchSphereMm;
        StepDiameterSearchSphereMm = ConfigSettings.StepDiameterSearchSphereMm;
        Search = ConfigSettings.Search;
        SphereProvider = _providersFactory.GetSphereProvider(_revitRepository);
    }

    public void UpdateConfigSettings() {
        ConfigSettings.ParamMaps = ParamMaps;
        ConfigSettings.Categories = Categories;
        ConfigSettings.ElementsProvider = ElementsProvider.Type;
        ConfigSettings.PositionProvider = PositionProvider.Type;
        ConfigSettings.SourceFile = FileProvider.Document.GetUniqId();
        ConfigSettings.TypeModels = TypeModels;
        ConfigSettings.MaxDiameterSearchSphereMm = MaxDiameterSearchSphereMm;
        ConfigSettings.StepDiameterSearchSphereMm = StepDiameterSearchSphereMm;
        ConfigSettings.Search = Search;
    }

    private IFileProvider GetFileProvider(string fileName) {
        var findedDoc = _revitRepository.FindDocumentsByName(fileName);
        return _providersFactory.GetFileProvider(_revitRepository, findedDoc);
    }
}

