using System.Text.RegularExpressions;

namespace RevitCorrectNamingCheck.Helpers;
internal static class NamingRulesHelper {
    /// <summary>
    /// Проверяет, содержит ли имя указанную метку раздела как отдельное слово.
    /// </summary>
    /// <remarks>
    /// Метка засчитывается, только если она отделена символами "_", ".", пробелом
    /// либо находится в начале/конце строки. Вхождение метки как части другого слова не засчитывается.
    /// </remarks>
    public static bool ContainsPart(string name, string part) {
        if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(part)) {
            return false;
        }

        string pattern = $@"(?:^|[_.\s]){Regex.Escape(part)}(?:[_.\s]|$)";
        return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Проверяет, содержит ли имя слово "связ".
    /// </summary>
    public static bool IsLinkWorkset(string name) {
        return name?.ToLower().Contains("связ") == true;
    }
}
