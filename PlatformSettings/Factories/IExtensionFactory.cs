using PlatformSettings.Model;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Factories {
    internal interface IExtensionFactory<T> where T : Extension {
        T Create(JToken token);
    }
}