using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal class TwoSourcesLinksProvider : ITwoSourceLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly IConfigProvider _configProvider;
    private readonly IOpenFolderDialogService _openFolderDialog;
    private readonly IOpenFolderDialog _rsOpenFolderDialog;

    public TwoSourcesLinksProvider(
        RevitRepository revitRepository,
        IConfigProvider configProvider,
        IOpenFolderDialogService openFolderDialog,
        IOpenFolderDialog rsOpenFolderDialog) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _openFolderDialog = openFolderDialog ?? throw new ArgumentNullException(nameof(openFolderDialog));
        _rsOpenFolderDialog = rsOpenFolderDialog ?? throw new ArgumentNullException(nameof(rsOpenFolderDialog));
    }


    public ILinksFromSource GetLocalLinks() {
        if(_openFolderDialog.ShowDialog()) {
            var config = _configProvider.GetUpdateLinksConfig();
            var settings = config.GetSettings(_revitRepository.Document)
                ?? config.AddSettings(_revitRepository.Document);
            settings.InitialFolderPath = _openFolderDialog.Folder.FullName;
            config.SaveProjectConfig();

            const string searchPattern = "*.rvt";

            return new LinksFromSource(_openFolderDialog.Folder.FullName,
                _openFolderDialog.Folders
                .SelectMany(dir => Directory.EnumerateFiles(
                    dir.FullName,
                    searchPattern,
                    SearchOption.AllDirectories))
                .Select(path => new Link(path))
                .ToArray());
        } else {
            throw new OperationCanceledException();
        }
    }

    public async Task<ILinksFromSource> GetServerLinksAsync() {
        if(_rsOpenFolderDialog.ShowDialog()) {
            var config = _configProvider.GetUpdateLinksConfig();
            var settings = config.GetSettings(_revitRepository.Document)
                ?? config.AddSettings(_revitRepository.Document);
            settings.InitialServerPath = _rsOpenFolderDialog.InitialDirectory;
            config.SaveProjectConfig();

            var models = new List<IFileModel>();
            foreach(var item in _rsOpenFolderDialog.Folders) {
                models.AddRange(await item.GetFilesAsync(SearchOption.AllDirectories));
            }
            return new LinksFromSource(_rsOpenFolderDialog.Folder.FullName,
                models.Select(m => new Link(m.FullName)).ToArray());
        } else {
            throw new OperationCanceledException();
        }
    }
}
