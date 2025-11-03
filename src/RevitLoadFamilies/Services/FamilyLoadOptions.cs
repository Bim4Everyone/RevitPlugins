using Autodesk.Revit.DB;

namespace RevitLoadFamilies.Services;
public class FamilyLoadOptions : IFamilyLoadOptions {
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
        overwriteParameterValues = true;
        return true;
    }

    public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues) {
        overwriteParameterValues = true;
        source = FamilySource.Family;
        return true;
    }
}
