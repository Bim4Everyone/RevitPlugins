using System;
using System.Collections.Generic;

using RevitExceptions = Autodesk.Revit.Exceptions;

namespace RevitLintelsManager.Models;

internal static class RevitElementRead {
    // Разрешает часто повторяющиеся справочные имена один раз за чтение.
    internal static string GetCachedName<TKey>(TKey id, IDictionary<TKey, string> names, Func<TKey, string> readName) {
        if(!names.TryGetValue(id, out string name)) {
            name = readName(id);
            names.Add(id, name);
        }
        return name;
    }

    // Ошибка отдельного поля не должна скрывать остальную информацию об экземпляре.
    internal static T ReadOptional<T>(Func<T> read) {
        try {
            return read();
        } catch(Exception exception) when(IsElementReadException(exception)) {
            return default;
        }
    }

    // Отделяет ошибки доступа к отдельному объекту от ошибок всего запроса.
    internal static bool IsElementReadException(Exception exception) {
        return exception is RevitExceptions.InvalidObjectException or RevitExceptions.InvalidOperationException
            or RevitExceptions.ArgumentException or RevitExceptions.InternalException;
    }
}
