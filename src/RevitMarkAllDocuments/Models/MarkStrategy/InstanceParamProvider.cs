using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkAllDocuments.Models;

internal class InstanceParamProvider : IParamProvider {
    private readonly List<FilterableParam> _params;

    public InstanceParamProvider(RevitRepository revitRepository, Category category) {
        _params = [.. revitRepository.GetFilterableParams(category)];
    }

    public IList<FilterableParam> GetParamsForFilterAndSort() {
        return[.. _params];
    }

    public IList<FilterableParam> GetParamsForMarks() {
        return [.. _params.Where(x => !x.IsTypeParam)];
    }

    public Element GetElementWithParam(Element element, FilterableParam param) {
        return param.IsTypeParam ? element.GetElementType() : element;
    }
}
