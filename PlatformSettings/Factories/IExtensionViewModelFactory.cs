using PlatformSettings.Model;
using PlatformSettings.ViewModels;

namespace PlatformSettings.Factories {
    internal interface IExtensionViewModelFactory {
        ExtensionViewModel Create(Extension extension);
    }
}