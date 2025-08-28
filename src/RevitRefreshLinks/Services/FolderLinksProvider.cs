using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal class FolderLinksProvider : ILocalSourceLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly IConfigProvider _configProvider;
    private readonly IOpenFileDialogService _openFileDialog;

    public FolderLinksProvider(
        RevitRepository revitRepository,
        IConfigProvider configProvider,
        IOpenFileDialogService openFileDialog) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _openFileDialog = openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));
    }


    public ILinksFromSource GetLocalLinks() {
        if(_openFileDialog.ShowDialog()) {
            var config = _configProvider.GetAddLinksFromFolderConfig();
            var settings = config.GetSettings(_revitRepository.Document)
                ?? config.AddSettings(_revitRepository.Document);
            settings.InitialFolderPath = _openFileDialog.File.DirectoryName;
            config.SaveProjectConfig();

            const string rvtExtension = ".rvt";

            return new LinksFromSource(settings.InitialFolderPath,
                _openFileDialog.Files
                    .Where(file => file.Extension.Equals(rvtExtension))
                    .Select(file => new Link(file.FullName))
                    .ToArray());
        } else {
            throw new OperationCanceledException();
        }
    }
}
