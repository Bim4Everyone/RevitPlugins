using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;
internal class FileProvider : IFileProvider {
    private readonly RevitRepository _revitRepository;

    public FileProvider(RevitRepository revitRepository, Document document) {
        _revitRepository = revitRepository;
        Document = document;
    }

    public Document Document { get; private set; }

    public ICollection<RevitElement> GetRevitElements(string typeModel) {
        return _revitRepository.GetRevitElements(Document, typeModel);
    }
}
