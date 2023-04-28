using pyRevitLabs.Json.Linq;

namespace RevitPlatformSettings.Model {
    internal class BuiltinExtension : Extension {
        public BuiltinExtension(JToken token)
            : base(token) {
        }
    }
}