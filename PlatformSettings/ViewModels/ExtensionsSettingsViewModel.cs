using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PlatformSettings.Model;
using PlatformSettings.Services;

namespace PlatformSettings.ViewModels {
    internal class ExtensionsSettingsViewModel : SettingsViewModel {
        private readonly IExtensionsService<BuiltinExtension> _builtinService;
        private readonly IExtensionsService<ThirdPartyExtension> _thirdPartyService;

        public ExtensionsSettingsViewModel(int id, int parentId, string settingsName,
            IExtensionsService<BuiltinExtension> builtinService,
            IExtensionsService<ThirdPartyExtension> thirdPartyService)
            : base(id, parentId, settingsName) {
            _builtinService = builtinService;
            _thirdPartyService = thirdPartyService;
        }

        private IEnumerable<Extension> GetExtensions() {
            return _thirdPartyService.GetExtensions()
                .OfType<Extension>()
                .Union(_builtinService.GetExtensions());
        }
    }
}