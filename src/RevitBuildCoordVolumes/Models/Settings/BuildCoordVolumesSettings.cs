using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class BuildCoordVolumesSettings {

    private readonly RevitRepository _revitRepository;

    public BuildCoordVolumesSettings(RevitRepository revitRepository, ConfigSettings configSettings) {
        _revitRepository = revitRepository;
        ConfigSettings = configSettings;
    }

    public ConfigSettings ConfigSettings { get; private set; }
    public AlgorithmType AlgorithmType { get; set; }
    public string TypeZone { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<Document> Documents { get; set; }
    public List<string> TypeSlabs { get; set; }
    public double SearchSide { get; set; }

    public void LoadConfigSettings() {
        AlgorithmType = ConfigSettings.AlgorithmType;
        TypeZone = GetTypeZone();
        ParamMaps = ConfigSettings.ParamMaps;
        Documents = GetDocuments();
        TypeSlabs = GetTypeSlabs();
        SearchSide = ConfigSettings.SearchSide;
    }

    public void UpdateConfigSettings() {
        ConfigSettings.AlgorithmType = AlgorithmType;
        ConfigSettings.TypeZone = TypeZone;
        ConfigSettings.ParamMaps = ParamMaps;
        ConfigSettings.Documents = Documents.Select(doc => doc.GetUniqId()).ToList();
        ConfigSettings.TypeSlabs = TypeSlabs;
        ConfigSettings.SearchSide = SearchSide;
    }


    private string GetTypeZone() {
        if(!string.IsNullOrEmpty(ConfigSettings.TypeZone)) {
            return ConfigSettings.TypeZone;
        }
        var paramMap = ConfigSettings.ParamMaps.FirstOrDefault();
        if(paramMap?.Type != ParamType.SectionParam || paramMap.SourceParam == null) {
            return ConfigSettings.TypeZone;
        }
        string zone = _revitRepository
            .GetTypeZones(paramMap.SourceParam)
            .FirstOrDefault(t => t == RevitConstants.TypeZone);
        return zone ?? ConfigSettings.TypeZone;
    }

    private List<Document> GetDocuments() {
        return ConfigSettings.Documents.Count == 0
            ? _revitRepository.GetAllDocuments().ToList()
            : ConfigSettings.Documents
            .Select(_revitRepository.FindDocumentsByName)
            .ToList();
    }

    private List<string> GetTypeSlabs() {
        return ConfigSettings.TypeSlabs.Count == 0
            ? _revitRepository.GetTypeSlabsByDocs(Documents)
                .Where(name => RevitConstants.SlabTypeNames.Any(type => name.Contains(type)))
                .ToList()
            : ConfigSettings.TypeSlabs;
    }
}
