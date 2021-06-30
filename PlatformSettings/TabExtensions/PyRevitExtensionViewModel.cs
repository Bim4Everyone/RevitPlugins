using System.ComponentModel;

using pyRevitLabs.PyRevit;

namespace PlatformSettings.TabExtensions {
    public class PyRevitExtensionViewModel : INotifyPropertyChanged {
        private readonly PyRevitExtensionDefinitionEx _pyRevitExtension;

        private bool _enabled;
        private bool _allowChangeEnabled;

        public PyRevitExtensionViewModel(PyRevitExtensionDefinitionEx pyRevitExtension) {
            _pyRevitExtension = pyRevitExtension;
        }

        public bool Enabled {
            get => _enabled;
            set {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public bool AllowChangeEnabled {
            get => _allowChangeEnabled;
            set {
                _allowChangeEnabled = value;
                OnPropertyChanged(nameof(AllowChangeEnabled));
            }
        }

        public PyRevitExtension InstalledExtension { get; set; }
        public IToggleExtension ToggleExtension { get; set; }
        public string ExtensionName { get => _pyRevitExtension.GetExtensionName(); }

        public bool BuiltIn { get => _pyRevitExtension.BuiltIn; }
        public bool DefaultEnabled { get => _pyRevitExtension.DefaultEnabled; }
        public bool RocketModeCompatible { get => _pyRevitExtension.RocketModeCompatible; }
        public string Name { get => _pyRevitExtension.Name; }
        public PyRevitExtensionTypes Type { get => _pyRevitExtension.Type; }
        public string Description { get => _pyRevitExtension.Description; }
        public string Author { get => _pyRevitExtension.Author; }
        public string AuthorProfile { get => _pyRevitExtension.AuthorProfile; }
        public string Url { get => _pyRevitExtension.Url; }
        public string Website { get => _pyRevitExtension.Website; }
        public string ImageUrl { get => _pyRevitExtension.ImageUrl; }
        public dynamic Templates { get => _pyRevitExtension.Templates; }
        public dynamic Dependencies { get => _pyRevitExtension.Dependencies; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
