using System;
using System.Security.Policy;

using dosymep.WPF.ViewModels;

using PlatformSettings.Model;
using PlatformSettings.Services;

namespace PlatformSettings.ViewModels {
    internal class ExtensionViewModel : BaseViewModel {
        private readonly Extension _extension;
        private readonly IPyRevitConfigService _pyRevitConfigService;

        private bool _isEnabled;

        public ExtensionViewModel(Extension extension, IPyRevitConfigService pyRevitConfigService) {
            _extension = extension;
            _pyRevitConfigService = pyRevitConfigService;
            _isEnabled = _pyRevitConfigService.IsEnabledExtension(_extension);
        }

        public bool IsEnabled {
            get => _isEnabled;
            set {
                if(AllowChangeEnabled) {
                    this.RaiseAndSetIfChanged(ref _isEnabled, value);
                }
            }
        }

        public bool AllowChangeEnabled => _extension.AllowChangeEnabled;

        public string Type => _extension.Type;
        public string Name => _extension.Name;
        public string Description => _extension.Description;

        public string Author => _extension.Author;
        public string AuthorProfile => _extension.AuthorProfile;

        public Url Url => _extension.Url;
        public Url Website => _extension.Website;

        public void SaveExtensionState() {
            if(IsEnabled == _pyRevitConfigService.IsEnabledExtension(_extension)) {
                return;
            }

            if(_extension is BuiltinExtension builtinExtension) {
                if(IsEnabled) {
                    _pyRevitConfigService.EnableExtension(builtinExtension);
                } else {
                    _pyRevitConfigService.DisableExtension(builtinExtension);
                }
            } else if(_extension is ThirdPartyExtension thirdPartyExtension) {
                if(IsEnabled) {
                    _pyRevitConfigService.InstallExtension(thirdPartyExtension);
                } else {
                    _pyRevitConfigService.RemoveExtension(thirdPartyExtension);
                }
            } else {
                throw new NotSupportedException($"Расширение не поддерживается \"{_extension.Name}\".");
            }
        }
    }
}