using pyRevitLabs.Json.Linq;

namespace RevitPlatformSettings.Model {
    internal class ThirdPartyExtension : Extension {
        public ThirdPartyExtension(JObject token, string category)
            : base(token, category) {
        }

        /// <summary>
        /// HACK: Могут быть проблемы, так как используется не предназначенное свойство.
        /// </summary>
        public override bool AllowChangeEnabled => !DefaultEnabled;
    }
}