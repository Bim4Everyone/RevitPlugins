using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Справочные данные исходного проекта для двух команд настроек и менеджера.</summary>
internal interface ISettingsCatalog {
    string ProjectTitle { get; }
    IEnumerable<RevitFamily> LintelFamilies { get; }
    IEnumerable<RevitFamily> OpeningFamilies { get; }
    IEnumerable<RevitWall> RevitWalls { get; }
    IEnumerable<string> PhaseNames { get; }

    /// <summary>Находит стадию исходного проекта по имени.</summary>
    /// <param name="name">Сохранённое имя стадии.</param>
    /// <returns>Идентификатор найденной стадии или null.</returns>
    ElementId GetPhaseIdByName(string name);

    /// <summary>Читает текущие идентификатор и имя назначенной стадии.</summary>
    /// <param name="id">Проверенный идентификатор стадии в исходном документе.</param>
    /// <returns>Неизменяемый ключ стадии.</returns>
    ConfigElementKey GetPhaseKey(ElementId id);

    /// <summary>Собирает типы допустимых категорий конструкций.</summary>
    /// <returns>Каталог типов текущего проекта без живых элементов.</returns>
    IReadOnlyList<StructureTypeSelection> GetStructureTypes();

    /// <summary>Освобождает каталоги запуска перед немодальной работой.</summary>
    void ReleaseCatalogs();
}
