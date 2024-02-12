using dosymep.Bim4Everyone;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.Factories {
    internal interface IFillAdskParamFactory {
        IFillParam Create(RevitParam adskParam, params RevitParam[] copyFromParam);
    }

    internal class FillAdskParamFactory : IFillAdskParamFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public FillAdskParamFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public IFillParam Create(RevitParam adskParam, params RevitParam[] copyFromParam) {
            return _resolutionRoot.Get<FillAdskParam>(
                new ConstructorArgument(nameof(adskParam), adskParam),
                new ConstructorArgument(nameof(copyFromParam), copyFromParam));
        }
    }
}