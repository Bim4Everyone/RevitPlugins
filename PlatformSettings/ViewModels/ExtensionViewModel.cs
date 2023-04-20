using System.Security.Policy;

using dosymep.WPF.ViewModels;

using PlatformSettings.Model;

namespace PlatformSettings.ViewModels {
    internal class ExtensionViewModel : BaseViewModel {
        private readonly Extension _extension;
        private bool _isEnabled;

        public ExtensionViewModel(Extension extension, bool isEnabled) {
            _extension = extension;
            _isEnabled = isEnabled;
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
    }
}