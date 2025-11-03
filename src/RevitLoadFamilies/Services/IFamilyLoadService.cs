using Autodesk.Revit.DB;

namespace RevitLoadFamilies.Services;
public interface IFamilyLoadService {
    bool LoadFamily(string filePath, Document document);
}
