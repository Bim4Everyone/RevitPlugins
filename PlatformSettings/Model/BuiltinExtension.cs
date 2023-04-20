using System.Diagnostics;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Model {
    internal class BuiltinExtension : Extension {
        public BuiltinExtension(JToken token)
            : base(token) {
        }
    }
}