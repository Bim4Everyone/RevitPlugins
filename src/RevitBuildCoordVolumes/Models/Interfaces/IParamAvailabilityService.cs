using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IParamAvailabilityService {
    /// <summary>
    /// Метод проверки наличия параметров в проекте
    /// </summary>    
    /// <remarks>
    /// В данном методе производится проверка наличия параметра в проекте
    /// </remarks>
    /// <returns>true - если параметр есть, false - если параметра нет</returns>
    bool IsParamExist(Document doc, string paramName);
    /// <summary>
    /// Метод получения описания параметра по имени параметра 
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получения описания
    /// </remarks>
    /// <returns>Definition параметра</returns>
    Definition GetDefinitionByName(Document doc, string paramName);
}
