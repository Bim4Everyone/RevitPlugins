using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Разделы 4–6: параметры СМР ФОП_Блок СМР, ФОП_Секция СМР, ФОП_Этаж СМР.
/// </summary>
internal static class SmrParameterRules {
    private const string _allowedCharacters = @"А-Яа-яЁё0-9 ,()\.\-";
    private const string _floorToken = @"(К|П|Т|ТХ|Д)?\d{2}";

    private static readonly Regex _allowedValue = new("^[" + _allowedCharacters + "]*$");

    private static readonly Regex _blockPattern =
        new(@"^(Корпус \d+\.\d+|Пристройка \d+\.\d+\.\d+|Автостоянка|ДОО( \d+)?|ОО( \d+)?)$");

    private static readonly Regex _sectionPattern = new(@"^(С\d+|ТБ\d+)$");

    private static readonly Regex _floorPattern = new(
        "^" + _floorToken + "((-|, ?)" + _floorToken + ")*( \\(" + _floorToken + "((-|, ?)" + _floorToken + ")*\\))? этаж$");

    /// <summary>
    /// Конструкции, которые под корпусом могут относиться к −2 этажу и ниже.
    /// </summary>
    private static readonly string[] _foundationLikeKeys = {
        "ФПлита", "БПодготовка", "ПСлой", "Засыпка", "риям", "утолщ", "Утолщ", "Ф_Изоляция"
    };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        var invalidValuesByParameter = CheckEmptyAndInvalidCharacters(context, findings);
        CheckBlocks(context, findings);
        CheckSections(context, findings, invalidValuesByParameter);
        CheckFloors(context, findings);
    }

    private static Dictionary<string, HashSet<string>> CheckEmptyAndInvalidCharacters(
        ModelCheckContext context,
        FindingCollection findings) {

        var invalidValuesByParameter = new Dictionary<string, HashSet<string>>();
        var parameters = new[] {
            (Name: ModelCheckContext.BlockParameter, Section: ReportSections.SmrBlock),
            (Name: ModelCheckContext.SectionParameter, Section: ReportSections.SmrSection),
            (Name: ModelCheckContext.FloorParameter, Section: ReportSections.SmrFloor)
        };
        foreach(var parameter in parameters) {
            var emptyElements = context.VorElements
                .Where(element => element.GetParameterOrEmpty(parameter.Name).Trim().Length == 0
                                  && !(parameter.Name == ModelCheckContext.SectionParameter
                                       && IsAnnex(element)))
                .ToList();
            if(emptyElements.Count > 0) {
                findings.Add(
                    parameter.Section,
                    "Параметр «" + parameter.Name + "» не заполнен у элементов ВОР.",
                    emptyElements.Select(element => element.Id),
                    context.FileName,
                    detail: ModelCheckContext.DescribeTypes(emptyElements, 4));
            }

            var elementIdsByInvalidValue = new OrderedMap<string, List<long>>();
            foreach(ElementData element in context.VorElements) {
                string value = element.GetParameterOrEmpty(parameter.Name);
                if(value.Length > 0 && !_allowedValue.IsMatch(value)) {
                    elementIdsByInvalidValue.GetOrAdd(value, () => new List<long>()).Add(element.Id);
                }
            }

            foreach(var invalidValue in elementIdsByInvalidValue) {
                if(!invalidValuesByParameter.ContainsKey(parameter.Name)) {
                    invalidValuesByParameter[parameter.Name] = new HashSet<string>();
                }

                invalidValuesByParameter[parameter.Name].Add(invalidValue.Key);
                string invalidCharacters = string.Concat(PythonFormat.SortOrdinal(
                    Regex.Matches(invalidValue.Key, "[^" + _allowedCharacters + "]")
                        .Cast<Match>()
                        .Select(match => match.Value)
                        .Distinct()));
                findings.Add(
                    parameter.Section,
                    "Параметр «" + parameter.Name + "» содержит недопустимые символы (латинские буквы и др.). "
                    + "Допускаются только кириллица, цифры, запятые, пробелы, скобки (Стандарт п. 3.2.2.1).",
                    invalidValue.Value,
                    context.FileName,
                    detail: "значение «" + invalidValue.Key + "», недопустимые символы «" + invalidCharacters + "»");
            }
        }

        return invalidValuesByParameter;
    }

    private static void CheckBlocks(ModelCheckContext context, FindingCollection findings) {
        var elementIdsByBadBlock = new OrderedMap<string, List<long>>();
        foreach(ElementData element in context.VorElements) {
            string block = element.GetParameterOrEmpty(ModelCheckContext.BlockParameter).Trim();
            bool isValid = block.Split(',').All(part =>
                _blockPattern.IsMatch(part.Trim()) || Regex.IsMatch(part.Trim(), @"^(Корпус|Пристройка) [\d\.]+-[\d\.]+$"));
            if(block.Length > 0 && !isValid) {
                elementIdsByBadBlock.GetOrAdd(block, () => new List<long>()).Add(element.Id);
            }
        }

        foreach(var badBlock in elementIdsByBadBlock) {
            findings.Add(
                ReportSections.SmrBlock,
                "Значение ФОП_Блок СМР не соответствует формату «Корпус <дом>.<корпус>», "
                + "«Пристройка <дом>.<корпус>.<№>», «Автостоянка».",
                badBlock.Value,
                context.FileName,
                detail: "«" + badBlock.Key + "»");
        }

        if(context.MainBlock.StartsWith("Корпус") && context.UndergroundFloorCount >= 2) {
            var deepElements = context.VorElements
                .Where(element => Regex.IsMatch(element.GetParameterOrEmpty(ModelCheckContext.FloorParameter), "^П0[2-9]")
                                  && !ModelCheckContext.ContainsAny(element.TypeName, _foundationLikeKeys)
                                  && element.GetParameterOrEmpty(ModelCheckContext.BlockParameter) != "Автостоянка")
                .ToList();
            if(deepElements.Count > 0) {
                findings.Add(
                    ReportSections.SmrBlock,
                    "Под корпусом 2 и более подземных этажа: конструкции −2 этажа и ниже относятся к «Автостоянке» "
                    + "(проектируются в файле корпуса), а у элементов указан корпус.",
                    deepElements.Select(element => element.Id),
                    context.FileName,
                    detail: ModelCheckContext.DescribeTypes(deepElements, 4));
            }
        }
    }

    private static void CheckSections(
        ModelCheckContext context,
        FindingCollection findings,
        Dictionary<string, HashSet<string>> invalidValuesByParameter) {

        var elementIdsByBadSection = new OrderedMap<string, List<long>>();
        var annexElementIds = new List<long>();
        foreach(ElementData element in context.VorElements) {
            string section = element.GetParameterOrEmpty(ModelCheckContext.SectionParameter).Trim();
            if(IsAnnex(element) && section.Length > 0) {
                annexElementIds.Add(element.Id);
                continue;
            }

            bool isValid = section.Split(',').All(part =>
                _sectionPattern.IsMatch(part.Trim()) || Regex.IsMatch(part.Trim(), @"^С\d+-С\d+$"));
            if(section.Length > 0 && !isValid) {
                elementIdsByBadSection.GetOrAdd(section, () => new List<long>()).Add(element.Id);
            }
        }

        if(annexElementIds.Count > 0) {
            findings.Add(
                ReportSections.SmrSection,
                "Для элементов пристроек параметр «ФОП_Секция СМР» не заполняется, а в модели он заполнен.",
                annexElementIds,
                context.FileName);
        }

        foreach(var badSection in elementIdsByBadSection) {
            // значения с недопустимыми символами уже в замечании выше
            if(invalidValuesByParameter.TryGetValue(ModelCheckContext.SectionParameter, out var invalidValues)
               && invalidValues.Contains(badSection.Key)) {
                continue;
            }

            findings.Add(
                ReportSections.SmrSection,
                "Значение ФОП_Секция СМР не соответствует формату «С<№>» / «ТБ<№>».",
                badSection.Value,
                context.FileName,
                detail: "«" + badSection.Key + "»");
        }
    }

    private static void CheckFloors(ModelCheckContext context, FindingCollection findings) {
        var elementIdsByBadFloor = new OrderedMap<string, List<long>>();
        foreach(ElementData element in context.VorElements) {
            string floor = element.GetParameterOrEmpty(ModelCheckContext.FloorParameter).Trim();
            if(floor.Length > 0 && !_floorPattern.IsMatch(floor)) {
                elementIdsByBadFloor.GetOrAdd(floor, () => new List<long>()).Add(element.Id);
            }
        }

        foreach(var badFloor in elementIdsByBadFloor) {
            findings.Add(
                ReportSections.SmrFloor,
                "Значение ФОП_Этаж СМР не соответствует формату «<№> этаж» (номер двумя цифрами, префиксы К/П/Т…; "
                + "статья «ВОР КР. Заполнение параметра ФОП_Этаж СМР»).",
                badFloor.Value,
                context.FileName,
                detail: "«" + badFloor.Key + "»");
        }

        CheckFloorsWithoutLevel(context, findings);
        FloorPositionRules.Check(context, findings);
    }

    /// <summary>
    /// Значение ФОП_Этаж СМР, которому не соответствует ни один уровень модели.
    /// </summary>
    private static void CheckFloorsWithoutLevel(ModelCheckContext context, FindingCollection findings) {
        List<LevelData> levels = context.Model.Levels;
        var levelFloors = new HashSet<string>(levels.Select(level => level.Name.Split('_')[0]));
        var elementIdsByUnknownFloor = new OrderedMap<string, List<long>>();
        foreach(ElementData element in context.VorElements) {
            string floor = element.GetParameterOrEmpty(ModelCheckContext.FloorParameter).Trim();
            bool isSingleFloor = floor.Length > 0
                                 && _floorPattern.IsMatch(floor)
                                 && !floor.Contains(",")
                                 && !floor.Contains("-")
                                 && !floor.Contains("(");
            if(isSingleFloor && levelFloors.Count > 0 && !levelFloors.Contains(floor)) {
                elementIdsByUnknownFloor.GetOrAdd(floor, () => new List<long>()).Add(element.Id);
            }
        }

        var levelNameByElementId = new Dictionary<long, string>();
        foreach(ElementData element in context.VorElements) {
            levelNameByElementId[element.Id] = element.LevelName ?? "";
        }

        foreach(var unknownFloor in elementIdsByUnknownFloor) {
            string floor = unknownFloor.Key;
            string floorNumber = Regex.Replace(floor, @"\D", "");
            string floorPrefix = Regex.Match(floor, "^[А-ЯA-Z]*").Value;
            var similarLevels = new List<string>();
            if(floorNumber.Length > 0) {
                foreach(LevelData level in levels) {
                    string levelHead = level.Name.Split('_')[0];
                    bool isStandardName = Regex.IsMatch(level.Name, @"^[А-ЯA-Z]{0,2}\d{2} этаж(_|$)");
                    bool hasSameNumber = Regex.IsMatch(
                        levelHead,
                        @"(?<![\d.,])0*" + int.Parse(floorNumber) + @"(?![\d.,])");
                    if(!isStandardName
                       && hasSameNumber
                       && (floorPrefix.Length == 0 || levelHead.Contains(floorPrefix))) {
                        similarLevels.Add(level.Name);
                    }
                }
            }

            var hostLevels = PythonFormat.MostCommon(unknownFloor.Value.Select(id =>
                levelNameByElementId.TryGetValue(id, out string levelName) && levelName.Length > 0
                    ? levelName
                    : "без уровня"));
            string detail = "значение «" + floor + "» — уровня с именем «" + floor + "_…» в модели нет";
            if(similarLevels.Count > 0) {
                detail += "; похожие уровни с нестандартным именем: "
                          + string.Join(", ", similarLevels.Take(4).Select(name => "«" + name + "»"));
            }

            detail += "; элементы стоят на уровнях: "
                      + string.Join(", ", hostLevels.Take(4).Select(level => "«" + level.Key + "» (" + level.Value + ")"));
            findings.Add(
                ReportSections.SmrFloor,
                "Значению «ФОП_Этаж СМР» (заполняется без отметки, напр. «13 этаж») не соответствует ни один уровень "
                + "модели. Имя уровня должно начинаться с того же этажа (Стандарт п. 2.9.3: «13 этаж_К3_С4_38.600»). "
                + "Причина — либо уровень назван не по Стандарту, либо в параметре указан не тот этаж; "
                + "исправить одно из двух.",
                unknownFloor.Value,
                context.FileName,
                FindingKinds.Clarify,
                detail);
        }
    }

    private static bool IsAnnex(ElementData element) {
        return element.GetParameterOrEmpty(ModelCheckContext.BlockParameter).StartsWith("Пристройка");
    }
}
