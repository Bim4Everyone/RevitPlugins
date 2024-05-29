using System;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Factories.ElementPositions;
using RevitSetLevelSection.Factories.LevelProviders;
using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.Factories {
    internal interface ILevelProviderFactoryFactory {
        ILevelProviderFactory Create(MainBimBuildPart mainBimBuildPart);
    }

    internal class LevelProviderFactoryFactory : ILevelProviderFactoryFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public LevelProviderFactoryFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public ILevelProviderFactory Create(MainBimBuildPart mainBimBuildPart) {
            var positionFactory = _resolutionRoot.Get<ElementPositionFactory>();
            var constructorArgument = new ConstructorArgument("positionFactory", positionFactory);
            
            if(mainBimBuildPart == MainBimBuildPart.ARPart) {
                return _resolutionRoot.Get<ARLevelProviderFactory>(constructorArgument);
            } else if(mainBimBuildPart == MainBimBuildPart.KRPart) {
                return _resolutionRoot.Get<KRLevelProviderFactory>(constructorArgument);
            } else if(mainBimBuildPart == MainBimBuildPart.VisPart) {
                return _resolutionRoot.Get<VISLevelProviderFactory>(constructorArgument);
            }

            throw new ArgumentException($"Переданный раздел \"{mainBimBuildPart.Name}\" не поддерживается.");
        }
    }
}