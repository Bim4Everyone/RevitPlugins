using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkAllDocuments.Models;

internal class TypeParamProvider : IParamProvider {
    private readonly List<FilterableParam> _params;

    public TypeParamProvider(RevitRepository revitRepository, Category category) {
        _params = [.. revitRepository.GetFilterableParams(category)];
    }

    public IList<FilterableParam> GetParamsForFilterAndSort() {
        return [.. _params.Where(x => x.IsTypeParam)];
    }

    public IList<FilterableParam> GetParamsForMarks() {
        return [.. _params.Where(x => x.IsTypeParam)];
    }

    public Element GetElementWithParam(Element element, FilterableParam param) {
        return element.GetElementType();
    }
}
