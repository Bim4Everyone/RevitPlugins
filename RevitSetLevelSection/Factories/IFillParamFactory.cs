using dosymep.Bim4Everyone;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.Factories {
    internal interface IFillParamFactory {
        IFillParam Create(ParamOption paramOption,
            IDesignOption designOption,
            IMassRepository massRepository);

        IFillParam Create(MainBimBuildPart buildPart,
            RevitParam revitParam,
            IZoneRepository zoneRepository);
    }

    internal class FillParamFactory : IFillParamFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public FillParamFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public IFillParam Create(ParamOption paramOption,
            IDesignOption designOption,
            IMassRepository massRepository) {

            return _resolutionRoot.Get<FillMassParam>(
                new ConstructorArgument(nameof(paramOption), paramOption),
                new ConstructorArgument(nameof(designOption), designOption),
                new ConstructorArgument(nameof(massRepository), massRepository));
        }

        public IFillParam Create(MainBimBuildPart buildPart,
            RevitParam revitParam,
            IZoneRepository zoneRepository) {
            
            ILevelProviderFactory levelProviderFactory =
                _resolutionRoot.Get<ILevelProviderFactoryFactory>().Create(buildPart);
            
            return _resolutionRoot.Get<FillLevelParam>(
                new ConstructorArgument(nameof(levelProviderFactory), levelProviderFactory),
                new ConstructorArgument(nameof(revitParam), revitParam),
                new ConstructorArgument(nameof(zoneRepository), zoneRepository));
        }
    }
}