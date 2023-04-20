using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using PlatformSettings.Model;
using PlatformSettings.Services;
using PlatformSettings.ViewModels;

namespace PlatformSettings.Factories {
    internal class ExtensionViewModelFactory : IExtensionViewModelFactory {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly IPyRevitConfigService _pyRevitConfigService;

        public ExtensionViewModelFactory(IResolutionRoot resolutionRoot, IPyRevitConfigService pyRevitConfigService) {
            _resolutionRoot = resolutionRoot;
            _pyRevitConfigService = pyRevitConfigService;
        }

        public ExtensionViewModel Create(Extension extension) {
            bool isEnabled = _pyRevitConfigService.IsEnabledExtension(extension);
            return _resolutionRoot.Get<ExtensionViewModel>(
                new ConstructorArgument(nameof(extension), extension),
                new ConstructorArgument(nameof(isEnabled), isEnabled));
        }
    }
}