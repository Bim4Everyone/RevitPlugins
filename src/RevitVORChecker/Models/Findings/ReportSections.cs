using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Findings;

/// <summary>
/// Разделы реестра замечаний в порядке вывода.
/// </summary>
internal static class ReportSections {
    public const string Collisions = "S01";
    public const string Integrity = "S02";
    public const string General = "S03";
    public const string SmrBlock = "S04";
    public const string SmrSection = "S05";
    public const string SmrFloor = "S06";
    public const string MaterialKeynote = "S07";
    public const string MaterialName = "S08";
    public const string MaterialDescription = "S09";
    public const string MaterialChapter = "S10";
    public const string MaterialWork = "S11";
    public const string MaterialUnit = "S12";
    public const string MaterialCountType = "S13";
    public const string Location = "S14";
    public const string Purpose = "S14b";
    public const string MaterialType = "S15";
    public const string Size = "S16";
    public const string Reinforcement = "S17";
    public const string AtsNumber = "S18";
    public const string AdditionalSorting = "S19";
    public const string ExcelComparison = "S20";
    public const string Ventilation = "S21";

    /// <summary>
    /// Ключ раздела → заголовок раздела в реестре.
    /// </summary>
    public static readonly OrderedMap<string, string> Titles = CreateTitles();

    public static string GetTitle(string section) {
        return Titles.TryGetValue(section, out string title) ? title : section;
    }

    /// <summary>
    /// Номер раздела в реестре: «S07» → «7», «S14b» → «14а».
    /// </summary>
    public static string GetNumber(string section) {
        return section.Replace("S", "").TrimStart('0').Replace("14b", "14а");
    }

    private static OrderedMap<string, string> CreateTitles() {
        return new OrderedMap<string, string> {
            [Collisions] = "Проверка на коллизии",
            [Integrity] = "Целостность объемов, примыкание конструкций, детализация",
            [General] = "Общие замечания (типоразмеры, материалы)",
            [SmrBlock] = "Заполнение параметров СМР (ФОП_Блок СМР)",
            [SmrSection] = "Заполнение параметров СМР (ФОП_Секция СМР)",
            [SmrFloor] = "Заполнение параметров СМР (ФОП_Этаж СМР)",
            [MaterialKeynote] = "Заполнение параметров Материал: ключевая пометка",
            [MaterialName] = "Заполнение параметров Материал: Имя",
            [MaterialDescription] = "Заполнение параметров Материал: Описание",
            [MaterialChapter] = "Заполнение параметров материала ФОП_МТР_Наименование главы",
            [MaterialWork] = "Заполнение параметров материала ФОП_МТР_Наименование работы",
            [MaterialUnit] = "Заполнение параметров материала ФОП_МТР_Единица измерения",
            [MaterialCountType] = "Заполнение параметров материала ФОП_МТР_ТИП подсчета",
            [Location] = "Заполнение параметров ФОП_ТИП_Местоположение",
            [Purpose] = "Заполнение параметров ФОП_ТИП_Назначение",
            [MaterialType] = "Заполнение параметров ФОП_ТИП_ТИП материала",
            [Size] = "Заполнение параметров ФОП_Размер",
            [Reinforcement] = "Заполнение параметров ФОП_ТИП_Армирования",
            [AtsNumber] = "Заполнение параметров ФОП_Номер АТС",
            [AdditionalSorting] = "Заполнение параметров ФОП_ТИП_Доп. Сортировка",
            [ExcelComparison] = "Соответствие выгруженных ВОР (Excel) модели",
            [Ventilation] = "Вентблоки и УЭРВ"
        };
    }
}
