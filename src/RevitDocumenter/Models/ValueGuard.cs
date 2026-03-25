using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RevitDocumenter.Models;
internal static class ValueGuard {
    /// <summary>
    /// Проверяет объект на null
    /// </summary>
    internal static T ThrowIfNull<T>(
        this T obj,
        [CallerArgumentExpression(nameof(obj))] string objName = null) {
        if(obj is null) {
            throw new ArgumentException($"{objName} cannot be null", objName);
        }
        return obj;
    }

    /// <summary>
    /// Проверяет строку на null или пустоту
    /// </summary>
    internal static string ThrowIfNullOrEmpty(
        this string str,
        [CallerArgumentExpression(nameof(str))] string objName = null) {
        str.ThrowIfNull(objName);

        if(str.Length == 0) {
            throw new ArgumentException($"{objName} cannot be empty", objName);
        }
        return str;
    }

    /// <summary>
    /// Проверяет коллекцию на null или пустоту
    /// </summary>
    internal static IEnumerable<T> ThrowIfNullOrEmpty<T>(
        this IEnumerable<T> collection,
        [CallerArgumentExpression(nameof(collection))] string collectionName = null) {
        collection.ThrowIfNull(collectionName);

        if(!collection.Any()) {
            throw new ArgumentException($"{collectionName} cannot be empty collection", collectionName);
        }
        return collection;
    }

    /// <summary>
    /// Проверяет несколько аргументов на null
    /// </summary>
    internal static void ThrowIfNull(
        params (object Obj, string ObjName)[] objects) {
        foreach(var (obj, objName) in objects) {
            obj.ThrowIfNull(objName);
        }
    }

    /// <summary>
    /// Проверяет несколько аргументов на null и пустые коллекции
    /// </summary>
    internal static void ThrowIfNullOrEmpty(
        params (object Obj, string ObjName)[] objects) {
        foreach(var (obj, objName) in objects) {
            obj.ThrowIfNull(objName);

            if(obj is IEnumerable enumerable && !enumerable.Cast<object>().Any()) {
                throw new ArgumentException($"{objName} cannot be empty collection", objName);
            }
        }
    }
}
