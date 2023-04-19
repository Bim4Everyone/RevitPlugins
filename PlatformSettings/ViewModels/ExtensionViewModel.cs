using System.Security.Policy;

using dosymep.WPF.ViewModels;

using PlatformSettings.Model;

namespace PlatformSettings.ViewModels {
    internal class ExtensionViewModel : BaseViewModel {
        private readonly Extension _extension;
        private bool _isSelected;

        public ExtensionViewModel(Extension extension) {
            _extension = extension;
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string Name => _extension.Name;
        public string Description => _extension.Description;

        public string Author => _extension.Author;
        public string AuthorProfile => _extension.AuthorProfile;

        public Url Url => _extension.Url;
        public Url Website => _extension.Website;
    }
}