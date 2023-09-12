using pyRevitLabs.Json.Linq;

namespace RevitPlatformSettings.Model {
    internal class ThirdPartyExtension : Extension {
        public ThirdPartyExtension(JObject token, string category)
            : base(token, category) {
        }

        public override bool AllowChangeEnabled => !DefaultEnabled;
    }
}