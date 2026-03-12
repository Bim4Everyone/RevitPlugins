using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ICategoryAvailabilityService {
    /// <summary>
    /// Метод проверки наличия параметров категории.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка наличия параметра в категории.
    /// </remarks>
    /// <param name="paramName">Имя параметра.</param>    
    /// <param name="category">Категория элемента.</param>
    /// <returns>
    /// True - если параметр есть в категории, False - если нет.
    /// </returns>
    bool IsParamAvailableInCategory(string paramName, Category category);
}
