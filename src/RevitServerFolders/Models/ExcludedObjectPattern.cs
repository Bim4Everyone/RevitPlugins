using System;

namespace RevitServerFolders.Models;

internal class ExcludedObjectPattern {
    /// <summary>
    /// Идентификатор подстроки.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Подстрока для скрытия файлов из списка моделей набора.
    /// </summary>
    public string Value { get; set; }
}
