using Autodesk.Revit.DB;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.LevelProviders;

namespace RevitSetLevelSection.Factories {
    internal interface ILevelProviderFactory {
        ILevelProvider Create(Element element);
    }
}