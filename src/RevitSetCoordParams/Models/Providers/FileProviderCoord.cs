using System.Collections.Generic;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class FileProviderCoord : IElementsProvider {
    private readonly RevitRepository _revitRepository;

    public FileProviderCoord(RevitRepository revitRepository, string fileName) {
        _revitRepository = revitRepository;
        FileName = fileName;
    }

    public ProviderType Type => ProviderType.CoordFileProvider;
    public string FileName { get; }
    public ICollection<RevitElement> GetRevitElements() {
        throw new System.NotImplementedException();
    }
}
