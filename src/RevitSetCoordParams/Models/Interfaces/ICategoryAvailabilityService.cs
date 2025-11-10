using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface ICategoryAvailabilityService {
    /// <summary>
    /// Метод проверки наличия параметров категории
    /// </summary>    
    /// <remarks>
    /// В данном методе производится проверка наличия параметра в категории
    /// </remarks>
    /// <returns>true - если параметр есть, false - если параметра нет</returns>
    bool IsParamAvailableInCategory(RevitParam param, Category category);
}
