using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class FileProviderCurrent : IFileProvider {
    private readonly RevitRepository _revitRepository;

    public FileProviderCurrent(RevitRepository revitRepository, Document document) {
        _revitRepository = revitRepository;
        Document = document;
    }

    public FileProviderType Type => FileProviderType.CurrentFileProvider;
    public Document Document { get; private set; }
    public ICollection<RevitElement> GetRevitElements() {
        throw new System.NotImplementedException();
    }
}
