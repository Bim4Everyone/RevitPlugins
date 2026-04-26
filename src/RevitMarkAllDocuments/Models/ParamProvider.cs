using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class ParamProvider {
    private readonly List<FilterableParam> _params;
    private readonly bool _isMarkForTypes;

    public ParamProvider(RevitRepository revitRepository, Category category, bool isMarkForTypes) {
        _params = [.. revitRepository.GetFilterableParams(category)];
        _isMarkForTypes = isMarkForTypes;
    }

    /// <summary>
    /// Метод получения параметров для фильтрации и сортировки.
    /// Для типоразмеров это только параметры типоразмера.
    /// Для экземпляров это параметры экземпляра и парамтеры типоразмера.
    /// </summary>
    public List<FilterableParam> GetParamsForFilterAndSort() {
        return _isMarkForTypes ? [.. _params.Where(x => x.IsTypeParam)] : _params;
    }

    /// <summary>
    /// Метод получения параметров, в которые может быть записано значение марки.
    /// Для типоразмеров это только параметры типоразмера. 
    /// Для экземпляров это только параметры экземпляра.
    /// </summary>
    public List<FilterableParam> GetParamsForMarks() {
        return [.. _params.Where(x => x.IsTypeParam == _isMarkForTypes)];
    }
}
