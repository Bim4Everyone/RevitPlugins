using Autodesk.Revit.DB;

namespace RevitLoadFamilies.Services.FamilyLoading;
internal interface IFamilyLoadService {
    bool LoadFamily(string filePath, Document document);
}
