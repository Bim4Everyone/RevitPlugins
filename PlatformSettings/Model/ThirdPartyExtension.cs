using System.Diagnostics;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Model {
    internal class ThirdPartyExtension : Extension {
        public ThirdPartyExtension(JObject token)
            : base(token) {
        }

        public override bool AllowChangeEnabled => !DefaultEnabled;
    }
}