using PlatformSettings.Model;

namespace PlatformSettings.Services {
    internal interface IPyRevitConfigService {
        bool IsEnabledExtension(Extension extension);
        
        void ToggleExtension(Extension extension);
        void EnableExtension(Extension extension);
        void DisableExtension(Extension extension);

        bool IsInstalledExtension(Extension extension);
        void InstallExtension(Extension extension);
        void RemoveExtension(Extension extension);
    }
}