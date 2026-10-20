using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabGeometryService {
    /// <summary>
    /// Получение геометрических данных плиты.
    /// </summary>
    /// <remarks>
    /// В данном методе определяется тип плиты, извлекаются реальные верхние грани
    /// и создаются проверочные солиды. Затем удаляются внутренние солиды,
    /// формируется список контуров и определяются верхние грани в зависимости
    /// от типа плиты. Все геометрические данные приводятся к заданной системе координат.
    /// </remarks>
    /// <param name="floor">Плита, геометрические данные которой необходимо получить.</param>
    /// <param name="transformFromDoc">Трансформация из системы координат исходного документа.</param>
    /// <returns>
    /// Объект <see cref="SlabGeometryData"/>, содержащий контуры плиты,
    /// верхние грани, проверочные солиды и тип плиты.
    /// </returns>
    SlabGeometryData GetSlabGeometryData(Floor floor, Transform transformFromDoc);
}
