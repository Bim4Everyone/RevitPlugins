using System.Collections.Generic;

using RevitPlatformSettings.Model;

namespace RevitPlatformSettings.Services {
    internal interface IExtensionsService<T> where T : Extension {
        IEnumerable<T> GetExtensions();
    }
}