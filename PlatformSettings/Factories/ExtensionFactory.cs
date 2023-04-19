using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using PlatformSettings.Model;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Factories {
    internal class ExtensionFactory<T> : IExtensionFactory<T> where T : Extension {
        private readonly IResolutionRoot _resolutionRoot;

        public ExtensionFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public T Create(JToken token) {
            return _resolutionRoot.Get<T>(new ConstructorArgument(nameof(token), token));
        }
    }
}