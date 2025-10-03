using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

using RevitServerFolders.Models;
using RevitServerFolders.Models.Rs;
using RevitServerFolders.Utils;

namespace RevitServerFolders.ViewModels.Rs;
internal class RsModelDataViewModel : RsModelObjectViewModel {
    private readonly ModelData _modelData;
    private readonly FolderContents _folderContents;

    public RsModelDataViewModel(IServerClient serverClient, ModelData modelData, FolderContents folderContents)
        : base(serverClient) {
        _modelData = modelData ?? throw new System.ArgumentNullException(nameof(modelData));
        _folderContents = folderContents ?? throw new System.ArgumentNullException(nameof(folderContents));

        Size = Extensions.BytesToString(_modelData.ModelSize);
    }

    public override string Name => _modelData.Name;

    public override string FullName => _serverClient.GetVisibleModelPath(_folderContents, _modelData);

    public override bool HasChildren => false;

    public override ModelObject GetModelObject() {
        return new RsFileModel(_modelData, _folderContents, _serverClient);
    }
}
