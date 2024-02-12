using RevitPlatformSettings.Model;
using RevitPlatformSettings.ViewModels;

namespace RevitPlatformSettings.Factories {
    internal interface IExtensionViewModelFactory {
        ExtensionViewModel Create(Extension extension);
    }
}