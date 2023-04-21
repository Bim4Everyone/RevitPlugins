using System.Collections.Generic;

using PlatformSettings.Model;

namespace PlatformSettings.Services {
    internal interface IExtensionsService<T> where T : Extension {
        IEnumerable<T> GetExtensions();
    }
}