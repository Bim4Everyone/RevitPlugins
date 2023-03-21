using Ninject;
using Ninject.Syntax;

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
                return _resolutionRoot.Get<ARLevelProviderFactory>();
            } else if(mainBimBuildPart == MainBimBuildPart.KRPart) {
                return _resolutionRoot.Get<KRLevelProviderFactory>();
            } else if(mainBimBuildPart == MainBimBuildPart.VisPart) {
                return _resolutionRoot.Get<VISLevelProviderFactory>();
            }

            return null;
        }
    }
}