using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using PlatformSettings.Factories;
using PlatformSettings.Model;
using PlatformSettings.Services;

namespace PlatformSettings.ViewModels {
    internal class ExtensionsSettingsViewModel : SettingsViewModel {
        private readonly IExtensionViewModelFactory _extensionFactory;
        private readonly IExtensionsService<BuiltinExtension> _builtinService;
        private readonly IExtensionsService<ThirdPartyExtension> _thirdPartyService;

        public ExtensionsSettingsViewModel(int id, int parentId, string settingsName,
            IExtensionViewModelFactory extensionFactory,
            IExtensionsService<BuiltinExtension> builtinService,
            IExtensionsService<ThirdPartyExtension> thirdPartyService)
            : base(id, parentId, settingsName) {
            _extensionFactory = extensionFactory;
            _builtinService = builtinService;
            _thirdPartyService = thirdPartyService;

            Extensions =
                new ObservableCollection<ExtensionViewModel>(GetExtensions()
                    .Select(item => _extensionFactory.Create(item)));
        }


        public ObservableCollection<ExtensionViewModel> Extensions { get; }

        private IEnumerable<Extension> GetExtensions() {
            return _thirdPartyService.GetExtensions()
                .OfType<Extension>()
                .Union(_builtinService.GetExtensions());
        }
    }
}