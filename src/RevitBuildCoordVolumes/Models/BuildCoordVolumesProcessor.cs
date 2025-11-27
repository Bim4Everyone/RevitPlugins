using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;
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

        var allSlabs = _revitRepository.GetSlabs(_settings.TypeSlabs);

        double minSlab = allSlabs
            .Select(s => s.Floor.GetBoundingBox().Max.Z)
            .Min();

        double maxSlab = allSlabs
            .Select(s => s.Floor.GetBoundingBox().Max.Z)
            .Max();

        var stops = allSlabs
            .Select(s => s.Floor.GetBoundingBox().Max.Z);

        var areaTypeParam = _settings.ParamMaps.First().SourceParam;
        var areas = _revitRepository.GetRevitAreas(areaType, areaTypeParam);

        var divider = new AreaDivider();

        double minimalSide = _revitRepository.Application.ShortCurveTolerance;
        double side = UnitUtils.ConvertToInternalUnits(_settings.SearchSide, UnitTypeId.Millimeters);

        foreach(var area in areas) {

            var polygons = divider.DivideArea((Area) area.Area, side, minimalSide, 1000000);

            foreach(var polygon in polygons) {



            }

            var p = allSlabs.First().ContourPoints;

            var po = allSlabs.First();

            var topfaces = HostObjectUtils.GetTopFaces(po.Floor);

            var elId = topfaces.First().ElementReferenceType;



            MessageBox.Show(elId.ToString());



        }



    }





}
