using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

using RevitServerFolders.Utils;

namespace RevitServerFolders.ViewModels.Rs;
internal sealed class RsServerDataViewModel : RsModelObjectViewModel {
    private bool _showFiles;

    public RsServerDataViewModel(IServerClient serverClient) : base(serverClient) {
        Size = "Расчет размера";
        _serverClient.GetFolderInfoAsync("|")
            .ContinueWith(t => {
                Size = Extensions.BytesToString(t.Result.Size);
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public override string Name => _serverClient.ServerName;
    public override string FullName => $"{_serverClient.ServerName}/{_serverClient.ServerVersion}";

    public override bool HasChildren => true;

    public bool ShowFiles {
        get => _showFiles;
        set => RaiseAndSetIfChanged(ref _showFiles, value);
    }

    protected override async Task<IEnumerable<RsModelObjectViewModel>> GetChildrenObjects() {
        var folderContents = await _serverClient.GetRootFolderContentsAsync(
            CancellationTokenSource.Token);
        List<RsModelObjectViewModel> content = [.. folderContents.Folders
            .Select(item => new RsFolderDataViewModel(_serverClient, item, folderContents) { ShowFiles = ShowFiles })];
        if(ShowFiles) {
            content.AddRange(folderContents.Models
                .Select(m => new RsModelDataViewModel(_serverClient, m, folderContents)));
        }
        return content;
    }
}
