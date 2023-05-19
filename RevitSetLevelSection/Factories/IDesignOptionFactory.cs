using Autodesk.Revit.DB;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.Factories {
    internal interface IDesignOptionFactory {
        DefaultDesignOption Create();
        DesignOptionAdapt Create(DesignOption designOption);
    }

    internal class DesignOptionFactory : IDesignOptionFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public DesignOptionFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public DefaultDesignOption Create() {
            return _resolutionRoot.Get<DefaultDesignOption>();
        }

        public DesignOptionAdapt Create(DesignOption designOption) {
            return _resolutionRoot.Get<DesignOptionAdapt>(
                new ConstructorArgument(nameof(designOption), designOption));
        }
    }
}