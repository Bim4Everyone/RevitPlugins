using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISpatialElementCheckService {
    /// <summary>
    /// Метод проверки зон.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка зон на избыточность, контур, неокруженность и отсуствие параметров.
    /// </remarks>
    /// <param name="settings">Основной класс с настройками плагина.</param>        
    /// <param name="revitRepository">Репозиторий Revit.</param>        
    /// <returns>
    /// Коллекция WarningElement.
    /// </returns>
    IReadOnlyCollection<WarningElement> CheckSpatialObjects(
        BuildCoordVolumeSettings settings, RevitRepository revitRepository);
}
