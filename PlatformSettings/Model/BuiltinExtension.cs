using System.Diagnostics;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Model {
    internal class BuiltinExtension : Extension {
        public BuiltinExtension(JToken token)
            : base(token) {
        }

        public override void EnableExtension() {
            Process.Start(GetApplicationPath(), $"enable {Name}");
        }

        public override void DisableExtension() {
            Process.Start(GetApplicationPath(), $"disable {Name}");
        }
    }
}