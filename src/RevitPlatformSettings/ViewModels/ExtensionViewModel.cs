using System.Security.Policy;

using dosymep.WPF.ViewModels;

using RevitPlatformSettings.Model;
using RevitPlatformSettings.Services;

namespace RevitPlatformSettings.ViewModels {
    internal class ExtensionViewModel : BaseViewModel {
        private readonly Extension _extension;
        private readonly IPyRevitExtensionsService _pyRevitExtensionsService;

        private bool _isEnabled;

        public ExtensionViewModel(Extension extension, IPyRevitExtensionsService pyRevitExtensionsService) {
            _extension = extension;
            _pyRevitExtensionsService = pyRevitExtensionsService;
            _isEnabled = _pyRevitExtensionsService.IsEnabledExtension(_extension);
        }

        public bool IsEnabled {
            get => _isEnabled;
            set {
                if(AllowChangeEnabled) {
                    this.RaiseAndSetIfChanged(ref _isEnabled, value);
                }
            }
        }

        public string Category => _extension.Category;

        public bool AllowChangeEnabled => _extension.AllowChangeEnabled;

        public string Type => _extension.Type;
        public string Name => _extension.Name;
        public string Description => _extension.Description;

        public string Author => _extension.Author;
        public string AuthorProfile => _extension.AuthorProfile;

        public Url Url => _extension.Url;
        public Url Website => _extension.Website;

        public void SaveExtensionState() {
            if(IsEnabled == _pyRevitExtensionsService.IsEnabledExtension(_extension)) {
                return;
            }

            if(IsEnabled) {
                _pyRevitExtensionsService.EnableExtension(_extension);
            } else {
                _pyRevitExtensionsService.DisableExtension(_extension);
            }
        }
    }
}