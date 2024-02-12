using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.Repositories {
    internal interface IMassRepository : IElementRepository<FamilyInstance> {
        Transform Transform { get; }

        bool HasIntersects(IDesignOption designOption);
        List<FamilyInstance> GetElements(IDesignOption designOption);
    }
}