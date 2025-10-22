using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class FileProviderLink : IFileProvider {
    private readonly RevitRepository _revitRepository;

    public FileProviderLink(RevitRepository revitRepository, Document document) {
        _revitRepository = revitRepository;
        Document = document;
    }

    public FileProviderType Type => FileProviderType.LinkFileProvider;
    public Document Document { get; private set; }
    public ICollection<RevitElement> GetRevitElements() {
        throw new System.NotImplementedException();
    }
}
