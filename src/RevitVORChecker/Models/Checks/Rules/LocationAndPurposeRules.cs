using System.Text.RegularExpressions;

using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Разделы 14 и 14а: ФОП_ТИП_Местоположение и ФОП_ТИП_Назначение.
/// </summary>
internal static class LocationAndPurposeRules {
    private static readonly HashSet<string> _allowedPurposes = new() {
        "Наружные", "Внутренние", "ЛЛБ", "Рампа/Пандус", "Диафрагма", "Приямки", "Перголы/Навесы", "Пергола/Навес",
        "Чаша бассейна", "Дефшов горизонтальный", "Дефшов вертикальный", "Термовкладыш", "Ребро жесткости",
        "Ребро жёсткости", "Приямки (Спуски в подвал)"
    };

    private static readonly HashSet<string> _wallPurposes = new() {
        "Наружные", "Внутренние", "ЛЛБ", "Рампа/Пандус", "Диафрагма", "Приямки", "Перголы/Навесы", "Пергола/Навес"
    };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        CheckLocation(context, findings);
        CheckPurpose(context, findings);
    }

    private static void CheckLocation(ModelCheckContext context, FindingCollection findings) {
        var problems = new GroupedElements();
        foreach(ElementData element in context.VorElements) {
            bool isThermalInsert = ModelCheckContext.IsThermalInsert(element);
            if(!(ModelCheckContext.IsInsulation(element) || isThermalInsert)) {
                continue;
            }

            string location = element.GetParameterOrEmpty(ModelCheckContext.LocationParameter).Trim();
            string purpose = element.GetParameterOrEmpty(ModelCheckContext.PurposeParameter).Trim();
            string locationBase = location.TrimEnd('_');
            if(locationBase != "Монолит" && locationBase != "Пенетрон") {
                problems.Add(
                    "ФОП_ТИП_Местоположение у изоляции/термовкладышей должен быть «Монолит» или «Пенетрон».",
                    "значение «" + (location.Length > 0 ? location : "пусто") + "»",
                    element);
            } else if(location.EndsWith("_") && purpose.Length == 0) {
                problems.Add(
                    "Не заполнен ФОП_ТИП_Назначение у изоляции: в столбце «Назначение» ВОР получается «" + location
                    + "» с подчёркиванием на конце — это признак незаполненного назначения (статья «КР ВОР. Проверка "
                    + "моделей», п. 7). Заполнить назначение (Дефшов вертикальный / горизонтальный / Ребро жесткости).",
                    null,
                    element);
            } else if(!location.EndsWith("_") && purpose.Length > 0) {
                problems.Add(
                    "В столбце «Назначение» ВОР значения слились без разделителя: Местоположение «" + location
                    + "» + Назначение «" + purpose + "» = «" + location + purpose + "». Проверить семейство изоляции "
                    + "(должно быть «" + location + "_" + purpose + "»).",
                    null,
                    element);
            }

            if(isThermalInsert && locationBase != "Монолит") {
                problems.Add("У термовкладыша ФОП_ТИП_Местоположение должен быть «Монолит».", null, element);
            }
        }

        problems.AddFindings(findings, ReportSections.Location, context.FileName);
    }

    private static void CheckPurpose(ModelCheckContext context, FindingCollection findings) {
        var problems = new GroupedElements();
        foreach(ElementData element in context.VorElements) {
            string typeName = element.TypeName;
            string purpose = element.GetParameterOrEmpty(ModelCheckContext.PurposeParameter).Trim();
            if(purpose.Length > 0 && !_allowedPurposes.Contains(purpose)) {
                problems.Add("Недопустимое значение ФОП_ТИП_Назначение.", "«" + purpose + "»", element);
            }

            if(Regex.IsMatch(typeName, "^(НН|ВН|Ф)_Стена")
               && element.Category == "Стены"
               && !_wallPurposes.Contains(purpose)) {
                problems.Add(
                    "У стен (тип «Стена») должно быть заполнено назначение через ВОР_Характеристики: Наружные / "
                    + "Внутренние / ЛЛБ / Диафрагма / Рампа/Пандус / Приямки / Перголы/Навесы.",
                    null,
                    element);
            }

            bool isJointInsulation = ModelCheckContext.IsInsulation(element)
                                     && (typeName.Contains("Шов") || (element.FamilyName ?? "").Contains("Шов"));
            if(isJointInsulation) {
                CheckJointPurpose(problems, element, typeName, purpose);
            }

            if(ModelCheckContext.IsThermalInsert(element) && purpose != "Термовкладыш") {
                problems.Add("У термовкладыша ФОП_ТИП_Назначение должно быть «Термовкладыш».", null, element);
            }
        }

        problems.AddFindings(findings, ReportSections.Purpose, context.FileName);
    }

    /// <summary>
    /// Шовная изоляция: «Стена» в имени типа → «Дефшов вертикальный», «Плита» → «Дефшов горизонтальный».
    /// </summary>
    private static void CheckJointPurpose(GroupedElements problems, ElementData element, string typeName, string purpose) {
        bool isWall = typeName.Contains("Стена");
        bool isSlab = typeName.Contains("Плита");
        string expectedPurpose = isWall && !isSlab
            ? "Дефшов вертикальный"
            : isSlab && !isWall
                ? "Дефшов горизонтальный"
                : null;
        if(purpose.Length == 0) {
            problems.Add(
                "У шовной изоляции не заполнено ФОП_ТИП_Назначение (Дефшов вертикальный / горизонтальный).",
                null,
                element);
        } else if(expectedPurpose != null && purpose != expectedPurpose && purpose != "Ребро жесткости") {
            problems.Add(
                "ФОП_ТИП_Назначение шовной изоляции не соответствует имени типа («Стена» → «Дефшов вертикальный», "
                + "«Плита» → «Дефшов горизонтальный»).",
                "«" + purpose + "» вместо «" + expectedPurpose + "»",
                element);
        }
    }
}
