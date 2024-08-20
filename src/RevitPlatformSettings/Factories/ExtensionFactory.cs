using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using pyRevitLabs.Json.Linq;

using RevitPlatformSettings.Model;

namespace RevitPlatformSettings.Factories {
    internal class ExtensionFactory<T> : IExtensionFactory<T> where T : Extension {
        private readonly IResolutionRoot _resolutionRoot;

        public ExtensionFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public T Create(JToken token, string category) {
            return _resolutionRoot.Get<T>(
                new ConstructorArgument(nameof(token), token),
                new ConstructorArgument(nameof(category), category));
        }
    }
}