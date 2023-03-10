using Autodesk.Revit.DB;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.ViewModels;

namespace RevitSetLevelSection.Factories {
    internal interface IViewModelFactory {
        LinkTypeViewModel Create(RevitLinkType revitLinkType);
        DesignOptionsViewModel Create(IDesignOption designOption, LinkInstanceRepository linkInstanceRepository);
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

        public DesignOptionsViewModel Create(IDesignOption designOption,
            LinkInstanceRepository linkInstanceRepository) {
            return _resolutionRoot.Get<DesignOptionsViewModel>(
                new ConstructorArgument(nameof(designOption), designOption),
                new ConstructorArgument(nameof(linkInstanceRepository), linkInstanceRepository));
        }
    }
}