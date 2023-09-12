using pyRevitLabs.Json.Linq;

using RevitPlatformSettings.Model;

namespace RevitPlatformSettings.Factories {
    internal interface IExtensionFactory<T> where T : Extension {
        T Create(JToken token, string category);
    }
}