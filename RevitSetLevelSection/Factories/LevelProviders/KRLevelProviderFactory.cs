using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.LevelProviders;

namespace RevitSetLevelSection.Factories.LevelProviders {
    internal class KRLevelProviderFactory : ILevelProviderFactory {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly IElementPositionFactory _positionFactory;

        public KRLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory) {
            _resolutionRoot = resolutionRoot;
            _positionFactory = positionFactory;
        }

        public ILevelProvider Create(Element element) {
            var elementPosition = _positionFactory.Create(element);
            var constructorArgument = new ConstructorArgument("elementPosition", elementPosition);

            if(element.InAnyCategory(GetLevelNearestProviderCategories())) {
                return _resolutionRoot.Get<LevelNearestProvider>(constructorArgument);
            } else if(element.InAnyCategory(BuiltInCategory.OST_StructuralFraming,
                          BuiltInCategory.OST_Floors,
                          BuiltInCategory.OST_StructuralTruss)) {
                return _resolutionRoot.Get<LevelMagicBottomProvider>(constructorArgument);
            } else if(element.InAnyCategory(BuiltInCategory.OST_StairsRuns,
                          BuiltInCategory.OST_StairsLandings)) {
                return _resolutionRoot.Get<LevelStairsProvider>(new ConstructorArgument("factory", this));
            }

            return null;
        }

        private IEnumerable<BuiltInCategory> GetLevelNearestProviderCategories() {
            yield return BuiltInCategory.OST_Walls;
            yield return BuiltInCategory.OST_StructuralColumns;
            yield return BuiltInCategory.OST_Stairs;
            yield return BuiltInCategory.OST_StructuralFoundation;
            yield return BuiltInCategory.OST_EdgeSlab;
        }
    }
}