using System;
#if REVIT_2020_OR_LESS
using System.Linq;
#endif

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
/// <summary>
/// Класс для нахождения высотной отметки относительно базовой точки проекта.
/// </summary>
internal class ElevationValueGetter : IValueGetter<DoubleParamValue> {
    private readonly IPointFinder _point;
    private readonly Document _activeDoc;

    /// <summary>
    /// Конструирует экземпляр класса для нахождения высотной отметки относительно базовой точки проекта.
    /// </summary>
    /// <param name="point">Провайдер точки относительно внутреннего начала проекта</param>
    /// <param name="activeDoc">Документ для получения базовой точки</param>
    /// <exception cref="ArgumentNullException">Исключение, если один из обязательных параметров null</exception>
    public ElevationValueGetter(IPointFinder point, Document activeDoc) {
        _point = point ?? throw new ArgumentNullException(nameof(point));
        _activeDoc = activeDoc ?? throw new ArgumentNullException(nameof(activeDoc));
    }


    public DoubleParamValue GetValue() {
        var basePoint = GetBasePoint();
        return new DoubleParamValue(_point.GetPoint().Z - basePoint.Position.Z);
    }


    private BasePoint GetBasePoint() {
#if REVIT_2020_OR_LESS
        return new FilteredElementCollector(_activeDoc)
            .WhereElementIsNotElementType()
            .OfClass(typeof(BasePoint))
            .OfType<BasePoint>()
            .First(p => !p.IsShared);
#else
        return BasePoint.GetProjectBasePoint(_activeDoc);
#endif
    }
}
