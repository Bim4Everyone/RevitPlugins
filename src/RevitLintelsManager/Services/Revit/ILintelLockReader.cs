using System.Collections.Generic;

namespace RevitLintelsManager.Models;

/// <summary>Читает действующие геометрические замки проёмов и перемычек.</summary>
internal interface ILintelLockReader {
    /// <summary>Подтверждает единственный центральный замок пары по идентификатору созданной зависимости.</summary>
    /// <param name="geometry">Контекст текущей операции после регенерации.</param>
    /// <param name="pair">Снимок пары перед созданием зависимости.</param>
    /// <param name="dependencyId">UniqueId размера, возвращённого Revit.</param>
    /// <returns>Совпадение прочитанной центральной зависимости с ожидаемой.</returns>
    bool ConfirmsCreatedLock(IGeometryReadContext geometry, OpeningLintelMatch pair, string dependencyId);

    /// <summary>Добавляет факты замков к парам в пределах одного чтения исходного документа.</summary>
    /// <param name="geometry">Контекст геометрии текущего API-вызова.</param>
    /// <param name="matches">Сопоставления текущего чтения.</param>
    /// <returns>Строки анализа с результатами чтения замков у пар.</returns>
    IReadOnlyList<OpeningLintelMatch> ReadLocks(IGeometryReadContext geometry,
        IReadOnlyList<OpeningLintelMatch> matches);
}
