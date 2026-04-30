using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal interface IParamProvider {
    /// <summary>
    /// Метод получения параметров для фильтрации и сортировки.
    /// Для типоразмеров это только параметры типоразмера.
    /// Для экземпляров это параметры экземпляра и парамтеры типоразмера.
    /// </summary>
    IList<FilterableParam> GetParamsForFilterAndSort();

    /// <summary>
    /// Метод получения параметров, в которые может быть записано значение марки.
    /// Для типоразмеров это только параметры типоразмера. 
    /// Для экземпляров это только параметры экземпляра.
    /// </summary>
    IList<FilterableParam> GetParamsForMarks();


    Element GetElementWithParam(Element element, FilterableParam param);
}

