using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using dosymep.Bim4Everyone;

namespace RevitCorrectNamingCheck.Helpers;
internal static class NamingRulesHelper {
    /// <summary>
    /// Проверяет, содержит ли имя указанный фрагмент (ID или Name).
    /// </summary>
    public static bool ContainsPart(string name, string part) {
        if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(part)) {
            return false;
        }

        string pattern = $@"(?:_|^){Regex.Escape(part)}(?:_|\.|\s|$)";
        return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Проверяет, содержит ли имя слово "связ".
    /// </summary>
    public static bool IsLinkWorkset(string name) {
        return name?.ToLower().Contains("связ") == true;
    }

    /// <summary>
    /// Считает количество совпадений среди частей.
    /// </summary>
    public static int CountMatches(string name, IEnumerable<string> parts) {
        return parts.Count(part => ContainsPart(name, part));
    }

    /// <summary>
    /// Проверяет, соответствует ли имя текущей BIM-модели.
    /// </summary>
    public static bool MatchesCurrentPart(string name, BimModelPart part) {
        return ContainsPart(name, part.Id) || ContainsPart(name, part.Name);
    }

    /// <summary>
    /// Проверяет, есть ли ровно одно совпадение.
    /// </summary>
    public static bool MatchesExactlyOnePart(string name, IEnumerable<string> parts) {
        return CountMatches(name, parts) == 1;
    }
}
