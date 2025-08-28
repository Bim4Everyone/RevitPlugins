using System;

using dosymep.Revit.ServerClient.DataContracts;

namespace RevitRefreshLinks.Models;
internal class RsFileModel : IFileModel {
    private readonly ModelData _modelData;

    public RsFileModel(ModelData modelData, IDirectoryModel directory) {
        _modelData = modelData ?? throw new System.ArgumentNullException(nameof(modelData));
        Directory = directory ?? throw new System.ArgumentNullException(nameof(directory));
        FullName = new Uri(new Uri(Directory.FullName.TrimEnd('/') + '/'), _modelData.Name).ToString();
    }

    public string Name => _modelData.Name;

    public string FullName { get; }

    public IDirectoryModel Directory { get; }

    public string DirectoryName => Directory.FullName;

    public long Length => _modelData.ModelSize;

    public bool Exists => true;
}
