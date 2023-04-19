using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Model {
    internal abstract class Extension {
        private readonly JToken _token;

        public Extension(JToken token) {
            _token = token;
        }

        public bool Builtin => GetValue<bool>();
        public string Name => GetValue<string>();
        public string Description => GetValue<string>();

        public string Author => GetValue<string>();
        public string AuthorProfile => GetValue<string>();

        public Url Url => new Url(GetValue<string>());
        public Url Website => new Url(GetValue<string>());

        public abstract void EnableExtension();
        public abstract void DisableExtension();

        protected T GetValue<T>([CallerMemberName] string propertyName = null) {
            return _token.Value<T>(ToSnakeCase(propertyName));
        }

        private string ToSnakeCase(string value) {
            if(value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            if(value.Length < 2) {
                return value;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(value[0]));
            for(int i = 1; i < value.Length; i++) {
                char c = value[i];
                if(char.IsUpper(c)) {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                } else {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        protected string GetApplicationPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "pyRevit-Master", "bin");
        }
    }
}