using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabAnalyzeSlopeService {
    /// <summary>
    /// Определение типа плиты по особенностям ее геометрии и способу задания уклона.
    /// </summary>
    /// <remarks>
    /// В данном методе проверяется наличие линейчатых граней, редактирование формы плиты
    /// и наличие линии уклона. В зависимости от результатов проверок плита классифицируется
    /// как линейчатая, наклонная плоская или плоская.
    /// </remarks>
    /// <param name="floor">Плита, тип которой необходимо определить.</param>
    /// <returns>
    /// Тип плиты: <see cref="SlabType.Ruled"/>, <see cref="SlabType.SlopedPlanar"/>
    /// или <see cref="SlabType.Planar"/>.
    /// </returns>
    SlabType GetSlabType(Floor floor);
}
