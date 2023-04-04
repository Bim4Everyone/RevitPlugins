using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.LevelProviders;

namespace RevitSetLevelSection.Factories.LevelProviders {
    internal class KRLevelProviderFactory : LevelProviderFactory {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly IElementPositionFactory _positionFactory;

        public KRLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory) {
            _resolutionRoot = resolutionRoot;
            _positionFactory = positionFactory;
        }

        protected override bool CanCreateImpl(Element element) {
            return _positionFactory.CanCreate(element)
                   && element.InAnyCategory(GetAllCategories());
        }

        protected override ILevelProvider CreateImpl(Element element) {
            var elementPosition = _positionFactory.Create(element);
            var constructorArgument = new ConstructorArgument("elementPosition", elementPosition);

            if(element.InAnyCategory(GetLevelNearestProviderCategories())) {
                return _resolutionRoot.Get<LevelNearestProvider>(constructorArgument);
            } else if(element.InAnyCategory(GetLevelMagicBottomProviderCategories())) {
                return _resolutionRoot.Get<LevelMagicBottomProvider>(constructorArgument);
            } else if(element.InAnyCategory(GetLevelStairsProviderCategories())) {
                return _resolutionRoot.Get<LevelStairsProvider>(new ConstructorArgument("factory", this));
            }

            throw new ArgumentException($"Переданный элемент \"{element.Id}\" с категорией \"{element.Category.Name}\" не поддерживается.");
        }
        
        private IEnumerable<BuiltInCategory> GetAllCategories() {
            return GetLevelNearestProviderCategories()
                .Union(GetLevelMagicBottomProviderCategories())
                .Union(GetLevelStairsProviderCategories());
        }

        private IEnumerable<BuiltInCategory> GetLevelMagicBottomProviderCategories() {
            yield return BuiltInCategory.OST_StructuralFraming;
            yield return BuiltInCategory.OST_Floors;
            yield return BuiltInCategory.OST_StructuralTruss;
        }

        private IEnumerable<BuiltInCategory> GetLevelStairsProviderCategories() {
            yield return BuiltInCategory.OST_StairsRuns;
            yield return BuiltInCategory.OST_StairsLandings;
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