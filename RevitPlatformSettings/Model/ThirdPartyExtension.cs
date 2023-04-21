using pyRevitLabs.Json.Linq;

namespace RevitPlatformSettings.Model {
    internal class ThirdPartyExtension : Extension {
        public ThirdPartyExtension(JObject token)
            : base(token) {
        }

        public override bool AllowChangeEnabled => !DefaultEnabled;
    }
}