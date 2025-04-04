using Autodesk.Revit.DB;

namespace RevitRoomExtrusion.Models {
    internal class FamilyLoadOptions : IFamilyLoadOptions {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(
            Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues) {
            source = FamilySource.Project;
            overwriteParameterValues = true;
            return true;
        }
    }
}
