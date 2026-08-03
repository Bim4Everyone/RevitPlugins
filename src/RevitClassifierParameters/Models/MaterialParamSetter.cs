using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClassifierParameters.Models;

public class MaterialParamSetter {

    /// <summary>
    /// Задает значения параметрам материалов 
    /// </summary>
    /// <param name="activeCodes">Выбранные пользователем коды для обработки</param>
    /// <param name="classifierWorks">Работы из классификатора</param>
    /// <param name="materialInPj">Материалы в проекте</param>
    /// <param name="forAr">Работа с архитектурными группами классификатора</param>
    public void SetParamValue(
        HashSet<string> activeCodes,
        List<WorkGroup> classifierWorks,
        List<Material> materialInPj,
        bool forAr) {

    }
}
