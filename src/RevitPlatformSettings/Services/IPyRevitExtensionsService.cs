using RevitPlatformSettings.Model;

namespace RevitPlatformSettings.Services {
    internal interface IPyRevitExtensionsService {
        bool IsEnabledExtension(Extension extension);
        
        void ToggleExtension(Extension extension);
        void EnableExtension(Extension extension);
        void DisableExtension(Extension extension);
    }
}