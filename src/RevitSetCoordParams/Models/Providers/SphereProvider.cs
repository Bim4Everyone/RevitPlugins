using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;
internal class SphereProvider : ISphereProvider {
    private readonly RevitRepository _revitRepository;

    public SphereProvider(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }
    public Solid GetSphere(XYZ location, double diameter) {
        return _revitRepository.GetSphereSolid(location, diameter);
    }
}
