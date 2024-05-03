using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitPlatformSettings.Model;
using RevitPlatformSettings.ViewModels;

namespace RevitPlatformSettings.Factories {
    internal class ExtensionViewModelFactory : IExtensionViewModelFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public ExtensionViewModelFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public ExtensionViewModel Create(Extension extension) {
            return _resolutionRoot.Get<ExtensionViewModel>(
                new ConstructorArgument(nameof(extension), extension));
        }
    }
}