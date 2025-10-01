using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services;
internal class DocTypesProvider : IDocTypesProvider {
    private readonly ICollection<DocTypeEnum> _docTypes;

    public DocTypesProvider(ICollection<DocTypeEnum> docTypes) {
        _docTypes = docTypes ?? throw new ArgumentNullException(nameof(docTypes));
    }

    public ICollection<DocTypeEnum> GetDocTypes() {
        return _docTypes;
    }
}
