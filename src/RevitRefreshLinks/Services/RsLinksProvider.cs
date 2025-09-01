using System;
using System.Linq;
using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal class RsLinksProvider : IServerSourceLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly IConfigProvider _configProvider;
    private readonly IOpenFileDialog _openFileDialog;

    public RsLinksProvider(
        RevitRepository revitRepository,
        IConfigProvider configProvider,
        IOpenFileDialog openFileDialog) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _openFileDialog = openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));
    }


    public async Task<ILinksFromSource> GetServerLinksAsync() {
        if(_openFileDialog.ShowDialog()) {
            var config = _configProvider.GetAddLinksFromServerConfig();
            var settings = config.GetSettings(_revitRepository.Document)
                ?? config.AddSettings(_revitRepository.Document);
            settings.InitialServerPath = _openFileDialog.File.DirectoryName;
            config.SaveProjectConfig();

            return await Task.FromResult(
                new LinksFromSource(settings.InitialServerPath,
                    _openFileDialog.Files
                        .Select(f => new Link(f.FullName))
                        .ToArray()));
        } else {
            throw new OperationCanceledException();
        }
    }
}
