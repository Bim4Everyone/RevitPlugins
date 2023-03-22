using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public bool CanCreate(Element element) {
            return element.InAnyCategory(GetAllCategories());
        }

        public IElementPosition Create(Element element) {
            if(element.InAnyCategory(GetElementTopPositionCategories())) {
                return _resolutionRoot.Get<ElementTopPosition>();
            } else if(element.InAnyCategory(GetElementBottomPositionCategories())) {
                return _resolutionRoot.Get<ElementBottomPosition>();
            }
            
            throw new ArgumentException($"Переданный элемент \"{element.Id}\" с категорией \"{element.Category.Name}\" не поддерживается.");
        }

        private IEnumerable<BuiltInCategory> GetAllCategories() {
            return GetElementTopPositionCategories()
                .Union(GetElementBottomPositionCategories());
        }

        private IEnumerable<BuiltInCategory> GetElementTopPositionCategories() {
            yield return BuiltInCategory.OST_StructuralFoundation;
            yield return BuiltInCategory.OST_EdgeSlab;
            yield return BuiltInCategory.OST_StructuralFraming;
            yield return BuiltInCategory.OST_Floors;
            yield return BuiltInCategory.OST_StructuralTruss;
        }
        
        private IEnumerable<BuiltInCategory> GetElementBottomPositionCategories() {
            yield return BuiltInCategory.OST_Walls;
            yield return BuiltInCategory.OST_StructuralColumns;
            yield return BuiltInCategory.OST_Stairs;
        }
    }
}