using PlatformSettings.Model;

namespace PlatformSettings.Services {
    internal interface IPyRevitConfigService {
        bool IsEnabledExtension(Extension extension);
    }
}