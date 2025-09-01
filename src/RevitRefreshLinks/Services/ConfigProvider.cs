using System;

using Ninject;
using Ninject.Syntax;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal class ConfigProvider : IConfigProvider {
    private readonly IResolutionRoot _resolutionRoot;

    public ConfigProvider(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
    }


    public AddLinksFromFolderConfig GetAddLinksFromFolderConfig() {
        return _resolutionRoot.Get<AddLinksFromFolderConfig>();
    }

    public AddLinksFromServerConfig GetAddLinksFromServerConfig() {
        return _resolutionRoot.Get<AddLinksFromServerConfig>();
    }

    public UpdateLinksConfig GetUpdateLinksConfig() {
        return _resolutionRoot.Get<UpdateLinksConfig>();
    }
}
