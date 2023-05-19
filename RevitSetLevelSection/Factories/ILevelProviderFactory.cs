using Autodesk.Revit.DB;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.LevelProviders;

namespace RevitSetLevelSection.Factories {
    internal interface ILevelProviderFactory {
        bool CanCreate(Element element);
        ILevelProvider Create(Element element);
        ILevelProvider CreateDefault(Element element);
    }
}