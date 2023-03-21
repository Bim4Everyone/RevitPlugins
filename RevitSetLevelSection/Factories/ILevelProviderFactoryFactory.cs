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
            if(mainBimBuildPart == MainBimBuildPart.ARPart) {
                var positionFactory = _resolutionRoot.Get<ARElementPositionFactory>();
                var constructorArgument = new ConstructorArgument("positionFactory", positionFactory);
                return _resolutionRoot.Get<ARLevelProviderFactory>(constructorArgument);
            } else if(mainBimBuildPart == MainBimBuildPart.KRPart) {
                var positionFactory = _resolutionRoot.Get<KRElementPositionFactory>();
                var constructorArgument = new ConstructorArgument("positionFactory", positionFactory);
                return _resolutionRoot.Get<KRLevelProviderFactory>(constructorArgument);
            } else if(mainBimBuildPart == MainBimBuildPart.VisPart) {
                var positionFactory = _resolutionRoot.Get<VISElementPositionFactory>();
                var constructorArgument = new ConstructorArgument("positionFactory", positionFactory);
                return _resolutionRoot.Get<VISLevelProviderFactory>(constructorArgument);
            }

            return null;
        }
    }
}