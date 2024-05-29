using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.Repositories;
using RevitSetLevelSection.ViewModels;

namespace RevitSetLevelSection.Factories {
    internal interface IViewModelFactory {
        LinkTypeViewModel Create(RevitLinkType revitLinkType);
        DesignOptionsViewModel Create(IDesignOption designOption, IMassRepository massRepository);

        FillParamViewModel Create(RevitParam revitParam);
        FillParamViewModel Create(ParamOption paramOption);
    }

    internal class ViewModelFactory : IViewModelFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public ViewModelFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public LinkTypeViewModel Create(RevitLinkType revitLinkType) {
            return _resolutionRoot.Get<LinkTypeViewModel>(
                new ConstructorArgument(nameof(revitLinkType), revitLinkType));
        }

        public DesignOptionsViewModel Create(IDesignOption designOption, IMassRepository massRepository) {
            return _resolutionRoot.Get<DesignOptionsViewModel>(
                new ConstructorArgument(nameof(designOption), designOption),
                new ConstructorArgument(nameof(massRepository), massRepository));
        }

        public FillParamViewModel Create(ParamOption paramOption) {
            return _resolutionRoot.Get<FillMassParamViewModel>(
                new ConstructorArgument(nameof(paramOption), paramOption));
        }

        public FillParamViewModel Create(RevitParam revitParam) {
            return _resolutionRoot.Get<FillLevelParamViewModel>(
                new ConstructorArgument(nameof(revitParam), revitParam));
        }
    }
}