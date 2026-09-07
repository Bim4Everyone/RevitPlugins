using System.Text.RegularExpressions;

namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Результат разбора имени типоразмера стены.
/// </summary>
internal readonly struct FacadeTypeNameParts {
    public FacadeTypeNameParts(string function, string material) {
        Function = function;
        Material = material;
    }

    /// <summary>
    /// Характеристика функции (со скобками), например "(ФМ)".
    /// </summary>
    public string Function { get; }

    /// <summary>
    /// Сокращение основного материала, например "ОК".
    /// </summary>
    public string Material { get; }

    /// <summary>
    /// Успешно ли извлечены обе подстроки.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Function) && !string.IsNullOrWhiteSpace(Material);
}

/// <summary>
/// Разбирает имя типоразмера стены вида
/// "(ФМ) ОК-5 15 (АРМСТК ФДШ-10 ОК-5) RAL #### Цоколь" на две ключевые подстроки:
/// характеристику функции в первых скобках ("(ФМ)")
/// и сокращение основного материала после скобок до первого дефиса ("ОК").
/// </summary>
internal class FacadeTypeNameParser {
    // Захватывает первые круглые скобки с содержимым и весь остаток строки после них.
    private static readonly Regex _regex =
        new(@"^\s*(?<function>\([^)]*\))\s*(?<rest>.*)$", RegexOptions.Compiled);

    /// <summary>
    /// Разбирает имя типоразмера стены.
    /// </summary>
    public FacadeTypeNameParts Parse(string typeName) {
        if(string.IsNullOrWhiteSpace(typeName)) {
            return default;
        }

        var match = _regex.Match(typeName);
        if(!match.Success) {
            return default;
        }

        string function = match.Groups["function"].Value.Trim();
        string material = ExtractMaterial(match.Groups["rest"].Value);

        return new FacadeTypeNameParts(function, material);
    }

    private static string ExtractMaterial(string rest) {
        if(string.IsNullOrWhiteSpace(rest)) {
            return string.Empty;
        }

        int dashIndex = rest.IndexOf('-');
        string material = dashIndex >= 0 ? rest.Substring(0, dashIndex) : rest;
        return material.Trim();
    }
}
