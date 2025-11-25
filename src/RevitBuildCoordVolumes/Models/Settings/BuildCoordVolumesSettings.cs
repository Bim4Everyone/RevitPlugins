using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class BuildCoordVolumesSettings {

    private readonly RevitRepository _revitRepository;
    private readonly ProvidersFactory _providersFactory;

    public BuildCoordVolumesSettings(RevitRepository revitRepository, ConfigSettings configSettings) {
        _revitRepository = revitRepository;
        ConfigSettings = configSettings;
        _providersFactory = new ProvidersFactory();
    }

    public ConfigSettings ConfigSettings { get; private set; }
    public List<Document> Documents { get; set; }
    public string TypeZone { get; set; }
    public IPositionProvider UpPositionProvider { get; set; }
    public IPositionProvider BottomPositionProvider { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<string> TypeSlabs { get; set; }
    public double SearchSide { get; set; }

    public void LoadConfigSettings() {
        Documents = GetDocuments();
        TypeZone = ConfigSettings.TypeZone;
        UpPositionProvider = _providersFactory.GetPositionProvider(_revitRepository, ConfigSettings.UpPositionProvider);
        BottomPositionProvider = _providersFactory.GetPositionProvider(_revitRepository, ConfigSettings.BottomPositionProvider);
        ParamMaps = ConfigSettings.ParamMaps;
        TypeSlabs = ConfigSettings.TypeSlabs;
        SearchSide = ConfigSettings.SearchSide;
    }

    public void UpdateConfigSettings() {
        ConfigSettings.Documents = Documents.Select(doc => doc.GetUniqId()).ToList();
        ConfigSettings.TypeZone = TypeZone;
        ConfigSettings.UpPositionProvider = UpPositionProvider.Type;
        ConfigSettings.BottomPositionProvider = BottomPositionProvider.Type;
        ConfigSettings.ParamMaps = ParamMaps;
        ConfigSettings.TypeSlabs = TypeSlabs;
        ConfigSettings.SearchSide = SearchSide;
    }

    private List<Document> GetDocuments() {
        return ConfigSettings.Documents.Count == 0
            ? _revitRepository.GetAllDocuments().ToList()
            : ConfigSettings.Documents
            .Select(_revitRepository.FindDocumentsByName)
            .ToList();
    }
}
