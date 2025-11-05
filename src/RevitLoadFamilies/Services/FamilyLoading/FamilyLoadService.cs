using Autodesk.Revit.DB;

namespace RevitLoadFamilies.Services.FamilyLoading;
internal class FamilyLoadService : IFamilyLoadService {
    private readonly FamilyLoadOptions _loadOptions;

    public FamilyLoadService() {
        _loadOptions = new FamilyLoadOptions();
    }

    public bool LoadFamily(string filePath, Document document) {
        try {
            return document.LoadFamily(filePath, _loadOptions, out _);
        } catch {
            return false;
        }
    }
}
