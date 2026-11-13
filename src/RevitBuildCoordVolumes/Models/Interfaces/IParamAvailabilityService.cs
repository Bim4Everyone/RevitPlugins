using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IParamAvailabilityService {
    /// <summary>
    /// Метод проверки наличия параметров в проекте.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка наличия параметра в проекте.
    /// </remarks>
    /// <param name="doc">Документ, где проводится проверка.</param>   
    /// <param name="paramName">Имя проверяемого параметра.</param> 
    /// <returns>
    /// True - если параметр есть, False - если параметра нет.
    /// </returns>
    bool IsParamExist(Document doc, string paramName);
    /// <summary>
    /// Метод получения описания параметра по имени параметра.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получения информации о параметре.
    /// </remarks>
    /// <param name="doc">Документ, где ищется описание.</param>   
    /// <param name="paramName">Имя параметра.</param> 
    /// <returns>
    /// Definition.
    /// </returns>
    Definition GetDefinitionByName(Document doc, string paramName);
}
