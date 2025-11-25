using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ICategoryAvailabilityService {
    /// <summary>
    /// Метод проверки наличия параметров категории
    /// </summary>    
    /// <remarks>
    /// В данном методе производится проверка наличия параметра в категории
    /// </remarks>
    /// <returns>true - если параметр есть, false - если параметра нет</returns>
    bool IsParamAvailableInCategory(string paramName, Category category);
}
