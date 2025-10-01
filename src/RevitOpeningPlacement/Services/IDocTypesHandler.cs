using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Интерфейс, предоставляющий методы по получению и конвертации БИМ разделов моделей
/// </summary>
internal interface IDocTypesHandler {
    /// <summary>
    /// Возвращает раздел проектирования заданного документа
    /// <param name="document">Документ Revit</param>
    /// </summary>
    DocTypeEnum GetDocType(Document document);

    /// <summary>
    /// Возвращает раздел проектирования заданного типа связи
    /// <param name="document">Документ Revit</param>
    /// </summary>
    DocTypeEnum GetDocType(RevitLinkType linkType);

    /// <summary>
    /// Возвращает коллекцию БИМ разделов, соответствующих заданному разделу проектирования
    /// </summary>
    /// <param name="docType">Раздел проектирования</param>
    /// <returns>Коллекция БИМ разделов</returns>
    ICollection<BimModelPart> GetBimModelParts(DocTypeEnum docType);
}
