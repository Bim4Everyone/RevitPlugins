using System.Globalization;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Разделы 15 и 16: ФОП_ТИП_Тип материала и ФОП_Размер.
/// </summary>
internal static class TypeParameterRules {
    /// <summary>
    /// Конструкции, у которых проверяется ФОП_Размер.
    /// </summary>
    private static readonly string[] _sizedKeys = {
        "Перекрытие", "Покрытие", "Стена", "Капитель", "ФПлита", "ЛПлощадка", "Рампа", "БПодготовка", "ПСлой",
        "Засыпка", "Парапет", "Подпорная", "Простенок"
    };

    /// <summary>
    /// Конструкции без ФОП_ТИП_Тип материала.
    /// </summary>
    private static readonly string[] _withoutMaterialTypeKeys = { "ПСлой", "Засыпка", "Свая", "Изол", "Термовкладыш" };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        CheckMaterialType(context, findings);
        CheckSize(context, findings);
    }

    private static void CheckMaterialType(ModelCheckContext context, FindingCollection findings) {
        var problems = new GroupedElements();
        var concreteElements = new HashSet<ElementData>(context.ConcreteElements);
        var checkedElements = context.ConcreteElements
            .Concat(context.VorElements.Where(element => !concreteElements.Contains(element)
                                                         && !ModelCheckContext.IsInsulation(element)
                                                         && (element.TypeName.Contains("(ЖБ")
                                                             || element.TypeName.Contains("БПодготовка")
                                                             || element.TypeName.Contains("ЦПР"))))
            .ToList();
        foreach(ElementData element in checkedElements) {
            string typeName = element.TypeName;
            string value = element.GetParameterOrEmpty(ModelCheckContext.MaterialTypeParameter).Trim();
            if(ModelCheckContext.ContainsAny(typeName, _withoutMaterialTypeKeys)) {
                continue;
            }

            if(typeName.Contains("ЦПР")) {
                if(!Regex.IsMatch(value, @"^М\d+$")) {
                    problems.Add(
                        "ФОП_ТИП_Тип материала у ЦПР должен быть М100 / М200 / М300.",
                        "«" + (value.Length > 0 ? value : "пусто") + "»",
                        element);
                }

                continue;
            }

            if(value.Length == 0) {
                problems.Add("ФОП_ТИП_Тип материала не заполнен (плагин «Параметры бетона», вкладка КР).", null, element);
                continue;
            }

            if(!Regex.IsMatch(value, @"^B\d+(\.\d+)?$")) {
                string cyrillicNote = value.StartsWith("В") ? " — первая буква кириллическая «В»" : "";
                problems.Add(
                    "ФОП_ТИП_Тип материала должен быть «B<класс>» с латинской B.",
                    "«" + value + "»" + cyrillicNote,
                    element);
                continue;
            }

            int bracketIndex = typeName.IndexOf('(');
            string typeTail = bracketIndex >= 0 ? typeName.Substring(bracketIndex + 1) : typeName;
            Match concreteClass = Regex.Match(typeTail, @"\b[BВ](\d+(?:[.,]\d+)?)");
            if(concreteClass.Success && concreteClass.Groups[1].Value.Replace(",", ".") != value.Substring(1)) {
                problems.Add(
                    "ФОП_ТИП_Тип материала не совпадает с классом бетона в имени типоразмера.",
                    "«" + value + "», в типе B" + concreteClass.Groups[1].Value,
                    element);
            }
        }

        problems.AddFindings(findings, ReportSections.MaterialType, context.FileName);
    }

    private static void CheckSize(ModelCheckContext context, FindingCollection findings) {
        var problems = new GroupedElements();
        foreach(ElementData element in context.VorElements) {
            string typeName = element.TypeName;
            bool isAssembledLanding = typeName.Contains("ЛПлощадка") && typeName.Contains("Сб");
            if(ModelCheckContext.IsInsulation(element)
               || !ModelCheckContext.ContainsAny(typeName, _sizedKeys)
               || isAssembledLanding) {
                continue;
            }

            string value = element.GetParameterOrEmpty(ModelCheckContext.SizeParameter).Trim();
            if(value.Length == 0) {
                problems.Add("ФОП_Размер не заполнен.", null, element);
                continue;
            }

            string expectedSize = GetExpectedSize(element, typeName);
            bool isExempt = typeName.Contains("Приям") || typeName.Contains("Перепад") || typeName.Contains("ФПлита");
            if(expectedSize != null && !isExempt && Regex.Replace(value, @"\D", "") != expectedSize) {
                problems.Add("ФОП_Размер не равен толщине конструкции.", "«" + value + "» вместо " + expectedSize + " мм", element);
            }
        }

        problems.AddFindings(findings, ReportSections.Size, context.FileName);
    }

    /// <summary>
    /// Толщина конструкции, мм: из параметра «Толщина» / «Площадка_Толщина» или из имени типа «…-200 (…)».
    /// </summary>
    private static string GetExpectedSize(ElementData element, string typeName) {
        string thickness = element.GetParameter("Толщина");
        if(string.IsNullOrEmpty(thickness)) {
            thickness = element.GetParameter("Площадка_Толщина");
        }

        if(!string.IsNullOrEmpty(thickness)) {
            string number = Regex.Replace(thickness, @"[^\d.,]", "").Replace(",", ".");
            bool isNumber = double.TryParse(number, NumberStyles.Float, PythonFormat.Invariant, out double parsed)
                            && Regex.IsMatch(number, @"^\d*\.?\d*$")
                            && number.Count(character => character == '.') <= 1
                            && number != ".";
            if(isNumber) {
                return ((long) Math.Round(parsed, MidpointRounding.ToEven)).ToString(PythonFormat.Invariant);
            }
        }

        Match sizeInName = Regex.Match(typeName, @"-(\d+)(?:[хx×]\d+)?\s*\(");
        return sizeInName.Success ? sizeInName.Groups[1].Value : null;
    }
}
