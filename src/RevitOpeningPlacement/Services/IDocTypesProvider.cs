using System.Collections.Generic;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Интерфейс, предоставляющий метод по получению разделов проекта, 
/// которые должны использоваться в алгоритмах плагина.
/// </summary>
internal interface IDocTypesProvider {
    /// <summary>
    /// Возвращает коллекцию разделов проектирования, 
    /// которым должны соответствовать связанные файлы, используемые в алгоритмах плагина.
    /// </summary>
    ICollection<DocTypeEnum> GetDocTypes();
}
