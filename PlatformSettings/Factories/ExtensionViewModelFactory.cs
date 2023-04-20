using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using PlatformSettings.Model;
using PlatformSettings.Services;
using PlatformSettings.ViewModels;

namespace PlatformSettings.Factories {
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