using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Предоставляет параметры помещений и их значения.</summary>
internal interface IParamService {
    /// <summary>Все доступные параметры помещений.</summary>
    IReadOnlyList<RevitParam> AllSpatialRevitParams { get; }
    /// <summary>Параметр имени по умолчанию.</summary>
    RevitParam? DefaultSpatialNameParam { get; }
    /// <summary>Параметр номера по умолчанию.</summary>
    RevitParam? DefaultSpatialNumberParam { get; }
    /// <summary>Параметр зависимого элемента по умолчанию.</summary>
    RevitParam? DefaultDependentParam { get; }
    /// <summary>Параметр поиска зависимого элемента по умолчанию.</summary>
    RevitParam? DefaultDependentSearchParam { get; }

    /// <summary>Возвращает отображаемое значение параметра помещения.</summary>
    /// <param name="spatialModel">Помещение, значение которого требуется получить.</param>
    /// <param name="filterParam">Параметр для отображения.</param>
    string GetParamValue(SpatialModel spatialModel, RevitParam? filterParam);

    /// <summary>Находит параметр Revit у элемента.</summary>
    /// <param name="revitParam">Описание искомого параметра.</param>
    /// <param name="element">Элемент, содержащий параметр.</param>
    /// <param name="param">Найденный параметр.</param>
    bool TryGetParam(RevitParam revitParam, Element? element, out Parameter? param);
}
