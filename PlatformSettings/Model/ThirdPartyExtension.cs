using System.Diagnostics;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Model {
    internal class ThirdPartyExtension : Extension {
        public ThirdPartyExtension(JObject token)
            : base(token) {
        }

        public bool AllowChangeEnabled => !Builtin;

        public override void EnableExtension() {
            Process.Start(GetApplicationPath(), $"extend ui {Name} {Url}")?.WaitForExit();
        }

        public override void DisableExtension() {
            Process.Start(GetApplicationPath(), $"delete {Name}")?.WaitForExit();
        }
    }
}