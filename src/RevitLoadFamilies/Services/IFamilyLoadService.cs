using Autodesk.Revit.DB;

namespace RevitLoadFamilies.Services;
internal interface IFamilyLoadService {
    bool LoadFamily(string filePath, Document document);
}
