using System.Linq;

using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class BuildCoordVolumesProcessor {

    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _settings;
    public BuildCoordVolumesProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    public void Run() {

        string areaType = _settings.TypeZone;
        var areaTypeParam = _settings.ParamMaps.First().SourceParam;

        var areas = _revitRepository.GetRevitAreas(areaType, areaTypeParam);




    }





}
