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
        public KRLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory)
            : base(resolutionRoot, positionFactory) {
        }

        protected override bool CanCreateImpl(Element element) {
            return base.CanCreateImpl(element)
                   || element.InAnyCategory(BuiltInCategory.OST_Floors);
        }

        protected override ILevelProvider CreateImpl(Element element) {
            if(element.InAnyCategory(BuiltInCategory.OST_Floors)) {
                return _resolutionRoot.Get<LevelMagicBottomProvider>(GetConstructorArgument(element));
            }

            return base.CreateImpl(element);
        }
    }
}