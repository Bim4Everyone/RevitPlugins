using System;

using Ninject;
using Ninject.Syntax;

using RevitRefreshLinks.Views;

namespace RevitRefreshLinks.Services;
internal class ExplorerWindowProvider : IDirectoriesExplorerWindowProvider, IFilesExplorerWindowProvider {
    private readonly IResolutionRoot _resolutionRoot;

    public ExplorerWindowProvider(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
    }


    public DirectoriesExplorerWindow GetDirectoriesExplorerWindow() {
        return _resolutionRoot.Get<DirectoriesExplorerWindow>();
    }

    public FilesExplorerWindow GetFilesExplorerWindow() {
        return _resolutionRoot.Get<FilesExplorerWindow>();
    }
}
