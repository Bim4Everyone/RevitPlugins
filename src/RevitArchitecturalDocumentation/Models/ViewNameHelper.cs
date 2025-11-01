using System;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitArchitecturalDocumentation.Models.Exceptions;

using View = Autodesk.Revit.DB.View;


namespace RevitArchitecturalDocumentation.Models;
internal class ViewNameHelper {
    private readonly ILocalizationService _localizationService;
    
    public ViewNameHelper(View view, ILocalizationService localizationService) {
        RevitView = view;
        ViewName = view?.Name;
        _localizationService = localizationService;

        GetViewCatName();
    }

    public string ViewName { get; set; }

    public string Prefix { get; set; }
    public string PrefixOfLevelBlock { get; set; }
    public int LevelNumber { get; set; }
    public string LevelNumberFormat { get; set; } = string.Empty;
    public string SuffixOfLevelBlock { get; set; }
    public string Suffix { get; set; }
    public string LevelPartOfName { get; set; }
    public string LevelNumberAsStr { get; set; }
    public View RevitView { get; set; }
    public string RevitViewCatName { get; set; } = string.Empty;


    /// <summary>
    /// Проверяет на наличие ошибок в имени и получает номер уровня, префикс и суффикс блока уровня и префикс и суффикс до и после блока этажа.
    /// Блок этажа - часть имени, отделенная "_" и имеющая слово "этаж"
    /// </summary>
    public void AnalyzeNGetNameInfo() {
        // "О_ПСО_05 этаж_Жилье Корпуса 1-3" или "О_ПСО_этаж 05_Жилье Корпуса 1-3"
        // [Prefix][PrefixOfLevelBlock] NUMBER [SuffixOfLevelBlock][Suffix]
        // [О_ПСО_]         []            5          [ этаж]       [_Жилье Корпуса 1-3] или
        // [О_ПСО_]      [этаж ]          5            []          [_Жилье Корпуса 1-3]

        AnalyzeNGetLevelNumber();

        // Получаем префикс и суффикс блока этажа относительно номера уровня, т.е. например
        // - в случае "05 этаж" - "" и " этаж"
        // - в случае "этаж 05" - "этаж " и ""
        GetPrefixNSuffixInString(LevelPartOfName, LevelNumberAsStr, out string levPrefix, out string levSuffix);
        PrefixOfLevelBlock = levPrefix;
        SuffixOfLevelBlock = levSuffix;

        // Получаем префикс и суффикс имени вида относительно блока этажа, т.е. например
        // - в случае "О_ПСО_05 этаж_Жилье Корпуса 1-3" - "О_ПСО_" и "_Жилье Корпуса 1-3"
        GetPrefixNSuffixInString(ViewName, LevelPartOfName, out string prefix, out string suffix);
        Prefix = prefix;
        Suffix = suffix;
    }


    /// <summary>
    /// Проверяет на наличие ошибок в имени и получает номер уровня.
    /// Номер уровня берется из части имени, отделенной "_" и имеющая слово "этаж"
    /// </summary>
    public void AnalyzeNGetLevelNumber() {
        // "AAAA_05 этаж_BBBB" или "AAAA_этаж 05_BBBB"

        // Проверяем, что строку имени вида передали и передали не пустую
        if(ViewName is null || ViewName.Length == 0) {
            throw new ViewNameException();
        }

        // Т.к. дальше будем резать строку на блоки через символ "_", то этот символ должен в строке быть
        if(!ViewName.Contains("_")) {
            throw new ViewNameException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.CutNameBySymbol")}" +
                $" \"_\" - {RevitViewCatName} \"{ViewName}\"");
        }

        string[] splittedName = ViewName.Split('_');

        // Ищем блок после резки по _, в котором содержится слово "этаж"
        // Получаем "05 этаж" или "этаж 05", либо бросаем исключение, что не нашли блок с ключевым словом "этаж"
        LevelPartOfName = splittedName.FirstOrDefault(o => o.IndexOf(_localizationService.GetLocalizedString("CreatingARDocsVM.Level"), 
                                                      StringComparison.OrdinalIgnoreCase) != -1) ??
            throw new ViewNameException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.WordLevelNeedNearLevelNumber")}" +
            $"- {RevitViewCatName} \"{ViewName}\"");

        // Проверяем есть ли цифры в блоке уровня
        var regex = new Regex(@"\d+");
        if(!regex.IsMatch(LevelPartOfName)) {
            throw new ViewNameException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.LevelNumberNotFound")}" +
                $" - {RevitViewCatName} \"{ViewName}\"");
        }

        // Получаем "05"
        LevelNumberAsStr = regex.Match(LevelPartOfName).Groups[0].Value;

        // Получаем int(5)
        if(!int.TryParse(LevelNumberAsStr, out int levelNumberAsInt)) {
            throw new ViewNameException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.LevelNumberNotDetect")}" +
                $" - {RevitViewCatName} \"{ViewName}\"");
        }
        LevelNumber = levelNumberAsInt;

        LevelNumberFormat = GetStringFormatOrDefault(LevelNumberAsStr);
    }



    /// <summary>
    /// Метод получает префикс и суффикс из строки на основе переданной части,которая должна быть в середине
    /// </summary>
    public void GetPrefixNSuffixInString(string stringForAnalyze, string keyString, out string prefix, out string suffix) {

        // "|XXXX|" - ключевая часть (keyString), которая должна быть по середине
        // "-AAAA-" или "-BBBB-" - часть имени, где не содержится ключевая часть
        // "-AAAA-|XXXX|-BBBB-"

        if(stringForAnalyze is null || keyString is null) {
            throw new ViewNameException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.PrefixNSuffixNotFound")}" +
                $"- {RevitViewCatName} \"{ViewName}\"");
        }

        if(stringForAnalyze.StartsWith(keyString)) {
            // Когда "|XXXX|-BBBB-"
            prefix = "";
            suffix = stringForAnalyze.Replace(keyString, "");
        } else if(stringForAnalyze.EndsWith(keyString)) {
            // Когда "-AAAA-|XXXX|"
            prefix = stringForAnalyze.Replace(keyString, "");
            suffix = "";
        } else {
            // Когда "-AAAA-|XXXX|-BBBB-"
            int n = stringForAnalyze.IndexOf(keyString);
            prefix = stringForAnalyze.Substring(0, n);

            // Применяем Regex для замены, чтобы заменить именно одно первое вхождение, чтобы закрыть ошибочные ситуации наименования
            // Например: "О_ПСО_ 02 этаж МОП Корпус 1-2" - проблема в пробеле между "О_ПСО_" и "02 этаж МОП Корпус 1-2"
            var regexForPrefix = new Regex(prefix);
            suffix = regexForPrefix.Replace(stringForAnalyze, "", 1).Replace(keyString, "");
        }
    }



    /// <summary>
    /// Получает строку формата на основе количества символов подаваемой строки с числом
    /// Строка формата представляет собой последовательность "{0:" + "0"*{Длина входной строки} + "}"
    /// </summary>
    public static string GetStringFormatOrDefault(string numAsString) {
        string format = string.Empty;

        if(!int.TryParse(numAsString, out _)) {
            return "{0:0}";
        }

        for(int i = 0; i < numAsString.Length; i++) { format += "0"; }
        return "{0:" + format + "}";
    }


    /// <summary>
    /// Получает имя типа вида, применяемое в тексте ошибок
    /// </summary>
    public void GetViewCatName() {
        if(RevitView is null) { return; }

        switch(RevitView.Category.GetBuiltInCategory()) {
            case BuiltInCategory.OST_Sheets:
                RevitViewCatName = _localizationService.GetLocalizedString("CreatingARDocsVM.Sheet");
                break;
            case BuiltInCategory.OST_Views:
                RevitViewCatName = _localizationService.GetLocalizedString("CreatingARDocsVM.View");
                break;
            case BuiltInCategory.OST_Schedules:
                RevitViewCatName = _localizationService.GetLocalizedString("CreatingARDocsVM.Spec");
                break;
        }
    }
}
