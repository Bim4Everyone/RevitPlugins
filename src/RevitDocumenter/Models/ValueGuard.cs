using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RevitDocumenter.Models;
internal static class ValueGuard {
    /// <summary>
    /// Проверяет объект на null
    /// </summary>
    internal static T ThrowIfNull<T>(
        this T obj,
        [CallerArgumentExpression(nameof(obj))] string objName = null)
        where T : class {
        if(obj is null) {
            throw new ArgumentException($"{objName} cannot be null");
        }
        return obj;
    }

    /// <summary>
    /// Проверяет число, что оно меньше заданного (по умолчанию 0)
    /// </summary>
    internal static T ThrowIfLessThan<T>(
        this T value,
        T minValue = default,
        [CallerArgumentExpression(nameof(value))] string paramName = null)
        where T : struct, IComparable<T> {
        if(value.CompareTo(minValue) < 0) {
            throw new ArgumentException(
                $"{paramName} with value '{value}' cannot be less than '{minValue}'");
        }
        return value;
    }

    /// <summary>
    /// Проверяет число, что оно меньше или равно заданному (по умолчанию 0)
    /// </summary>
    internal static T ThrowIfLessOrEqualThan<T>(
        this T value,
        T minValue = default,
        [CallerArgumentExpression(nameof(value))] string paramName = null)
        where T : struct, IComparable<T> {
        if(value.CompareTo(minValue) <= 0) {
            throw new ArgumentException(
                $"{paramName} with value '{value}' must be greater than '{minValue}'");
        }
        return value;
    }

    /// <summary>
    /// Проверяет строку на null или пустоту
    /// </summary>
    internal static string ThrowIfNullOrEmpty(
        this string str,
        [CallerArgumentExpression(nameof(str))] string objName = null) {
        str.ThrowIfNull(objName);

        if(str.Length == 0) {
            throw new ArgumentException($"{objName} cannot be empty");
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
            throw new ArgumentException($"{collectionName} cannot be empty collection");
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
                throw new ArgumentException($"{objName} cannot be empty collection");
            }
        }
    }

    /// <summary>
    /// Проверяет есть ли файл по указанному пути
    /// </summary>
    internal static string ThrowIfFileNotExist(this string path) {
        if(!File.Exists(path)) {
            throw new ArgumentException($"The file does not exist at the path:{path}");
        }
        return path;
    }
}
