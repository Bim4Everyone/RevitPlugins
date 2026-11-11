using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitEnumerateBySpline.Models.Interfaces;

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
    string GetParamValue(SpatialModel spatialModel, RevitParam? filterParam);

    /// <summary>Находит параметр Revit у элемента.</summary>
    bool TryGetParam(RevitParam revitParam, Element? element, out Parameter? param);
}
