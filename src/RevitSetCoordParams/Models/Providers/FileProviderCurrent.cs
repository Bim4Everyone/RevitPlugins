using System.Collections.Generic;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class FileProviderCurrent : IElementsProvider {
    private readonly RevitRepository _revitRepository;
    private readonly string _fileName;

    public FileProviderCurrent(RevitRepository revitRepository, string fileName) {
        _revitRepository = revitRepository;
        _fileName = fileName;
    }

    public ProviderType Type => ProviderType.CurrentFileProvider;

    public ICollection<RevitElement> GetRevitElements() {
        throw new System.NotImplementedException();
    }
}
