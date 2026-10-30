using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Раздел 3: имена типоразмеров монолитных конструкций; раздел 8: материал «Бетон» у монолита.
/// </summary>
internal static class TypeNameRules {
    /// <summary>
    /// Кириллические «В» (класс бетона) и «Ф» (морозостойкость) перед цифрой в скобках имени типа.
    /// </summary>
    public static readonly Regex CyrillicClassLetters = new(@"(?<![A-Za-zА-Яа-яЁё])[ВФ]\d");

    private static readonly Dictionary<string, string> _cyrillicLetterProblems = new() {
        ["В"] = "класс бетона записан кириллической «В» (напр. «В30») — заменить на латинскую «B»",
        ["Ф"] = "марка по морозостойкости записана кириллической «Ф» (напр. «Ф150») — заменить на латинскую «F»"
    };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        CheckTypeNames(context, findings);
        if(context.IsConcreteMaterialRequired) {
            CheckConcreteMaterials(context, findings);
        }
    }

    private static void CheckTypeNames(ModelCheckContext context, FindingCollection findings) {
        var elementsByProblem = new OrderedMap<string, List<ElementData>>();
        void AddProblem(string problem, ElementData element) {
            elementsByProblem.GetOrAdd(problem, () => new List<ElementData>()).Add(element);
        }

        foreach(ElementData element in context.ConcreteElements) {
            string typeName = element.TypeName;
            int bracketIndex = typeName.IndexOf('(');
            string insideBrackets = bracketIndex >= 0 ? typeName.Substring(bracketIndex + 1) : "";
            if(Regex.IsMatch(typeName, @"\s{2,}")) {
                AddProblem("двойной пробел в имени типоразмера", element);
            }

            var cyrillicLetters = PythonFormat.SortOrdinal(CyrillicClassLetters.Matches(insideBrackets)
                .Cast<Match>()
                .Select(match => match.Value.Substring(0, 1))
                .Distinct());
            if(cyrillicLetters.Count > 0) {
                AddProblem(string.Join("; ", cyrillicLetters.Select(letter => _cyrillicLetterProblems[letter])), element);
            }

            bool isSizeExempt = Regex.IsMatch(element.FamilyName ?? "", "утолщени|приям|перепад", RegexOptions.IgnoreCase)
                                || element.BuiltInCategory == "OST_StructConnections"
                                || element.Category == "Соединения несущих конструкций";
            bool hasNoSize = Regex.IsMatch(typeName, @"^(Ф|НН|ВН)_[^-(]+_\(")
                             || Regex.IsMatch(typeName, @"^(Ф|НН|ВН)_(ФПлита|Перекрытие|Стена|Покрытие|Капитель|БПодготовка)\s*\(");
            if(!isSizeExempt && hasNoSize) {
                AddProblem("нет размера конструкции («<Тип>-<размер> (…)»)", element);
            }

            bool isReinforcedConcrete = typeName.Contains("ЖБ");
            if(isReinforcedConcrete
               && !Regex.IsMatch(typeName, @"\bB\d")
               && !CyrillicClassLetters.IsMatch(insideBrackets)) {
                AddProblem("нет класса бетона B", element);
            }

            if(isReinforcedConcrete
               && !(Regex.IsMatch(typeName, @"\bF\d+") && Regex.IsMatch(typeName, @"\bW\d+"))) {
                AddProblem("нет марок F и W", element);
            }
        }

        foreach(var problem in elementsByProblem) {
            findings.Add(
                ReportSections.General,
                "Наименование типоразмеров не соответствует Стандарту (п. 3.4.1): " + problem.Key + ".",
                problem.Value.Select(element => element.Id),
                context.FileName,
                detail: ModelCheckContext.DescribeTypes(problem.Value));
        }
    }

    private static void CheckConcreteMaterials(ModelCheckContext context, FindingCollection findings) {
        var elementsWithOldMaterial = context.ConcreteElements
            .Where(element => context.GetVorMaterials(element).Any(material =>
                                  !IsConcreteName(material) && material.Keynote != "г02")
                              && !element.TypeName.Contains("БПодготовка")
                              && !element.TypeName.Contains("ЦПР"))
            .ToList();
        if(elementsWithOldMaterial.Count > 0) {
            findings.Add(
                ReportSections.MaterialName,
                "Для монолитных конструкций (стадия П после 07.03.2025) должен быть назначен материал «Бетон» "
                + "или «Бетон с пенетроном».",
                elementsWithOldMaterial.Select(element => element.Id),
                context.FileName,
                detail: ModelCheckContext.DescribeTypes(elementsWithOldMaterial));
        }

        var penetronMismatch = context.ConcreteElements
            .Where(element => element.TypeName.Contains("Пенетрон") != context.GetVorMaterials(element)
                                  .Any(material => (material.Name ?? "").IndexOf("пенетрон", StringComparison.OrdinalIgnoreCase) >= 0)
                              && !element.TypeName.Contains("БПодготовка"))
            .ToList();
        if(penetronMismatch.Count > 0) {
            findings.Add(
                ReportSections.MaterialName,
                "Материал не соответствует типоразмеру: «Пенетрон» есть в имени типа, а материал «Бетон» (или наоборот).",
                penetronMismatch.Select(element => element.Id),
                context.FileName,
                detail: ModelCheckContext.DescribeTypes(penetronMismatch));
        }
    }

    private static bool IsConcreteName(MaterialData material) {
        string name = PythonFormat.NormalizeText(material.Name).ToLowerInvariant();
        return name is "бетон" or "бетон с пенетроном" or "железобетон" or "железобетон с пенетроном";
    }
}
