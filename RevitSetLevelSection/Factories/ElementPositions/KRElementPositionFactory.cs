using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Factories.ElementPositions {
    internal class KRElementPositionFactory : IElementPositionFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public KRElementPositionFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public IElementPosition Create(Element element) {
            if(element.InAnyCategory(BuiltInCategory.OST_StructuralFoundation,
                   BuiltInCategory.OST_EdgeSlab,
                   BuiltInCategory.OST_StructuralFraming,
                   BuiltInCategory.OST_Floors,
                   BuiltInCategory.OST_StructuralTruss)) {
                return _resolutionRoot.Get<ElementTopPosition>();
            } else if(element.InAnyCategory(BuiltInCategory.OST_Walls,
                          BuiltInCategory.OST_StructuralColumns,
                          BuiltInCategory.OST_Stairs)) {
                return _resolutionRoot.Get<ElementBottomPosition>();
            }
            
            return null;
        }
    }
}