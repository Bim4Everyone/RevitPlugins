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
    internal class ARLevelProviderFactory : LevelProviderFactory {
        public ARLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory)
            : base(resolutionRoot, positionFactory) {
        }
    }
}