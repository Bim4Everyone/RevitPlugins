using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Разделы 7–13: параметры материалов ВОР (ключевая пометка, имя, описание, глава, работа, единица, тип подсчёта).
/// </summary>
internal static class MaterialRules {
    /// <summary>
    /// Главы по умолчанию, если в Классификаторе нет наименования главы.
    /// </summary>
    private static readonly Dictionary<string, string> _defaultChapters = new() {
        ["г02.02"] = "Устройство фундамента",
        ["г02.03"] = "Конструкции до отм. +/-0,000",
        ["г02.04"] = "Конструкции выше отм. +/-0,000"
    };

    /// <summary>
    /// Единица измерения → значение ФОП_МТР_Тип подсчета.
    /// </summary>
    private static readonly Dictionary<string, string> _countTypeByUnit = new() {
        ["м"] = "1",
        ["м²"] = "2",
        ["м³"] = "3",
        ["шт."] = "4",
        ["шт"] = "4"
    };

    private static readonly Regex _latinLetter = new("[A-Za-z]");

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        foreach(long materialId in context.UsedMaterialIds.OrderBy(id => id)) {
            MaterialData material = context.Materials[materialId];
            if(ModelCheckContext.IsConcreteMaterial(material)) {
                CheckConcreteMaterial(context, findings, material);
            } else {
                CheckWorkMaterial(context, findings, material);
            }
        }
    }

    /// <summary>
    /// Бетон / железобетон: код «г02», описание = имя, глава и работа не заполняются, единица м³, тип подсчёта 3.
    /// </summary>
    private static void CheckConcreteMaterial(
        ModelCheckContext context,
        FindingCollection findings,
        MaterialData material) {

        string name = material.Name ?? "";
        string keynote = PythonFormat.Trim(material.Keynote);
        string reference = "Материал «" + name + "»";
        string fileName = context.FileName;
        if(keynote != "г02") {
            findings.Add(
                ReportSections.MaterialKeynote,
                reference + ": ключевая пометка «" + keynote + "», для бетона/железобетона должна быть «г02».",
                null,
                fileName);
        }

        if(PythonFormat.NormalizeText(material.Description) != PythonFormat.NormalizeText(name)) {
            findings.Add(
                ReportSections.MaterialDescription,
                reference + ": «Описание» = «" + (material.Description ?? "")
                + "», для бетона должно совпадать с именем материала.",
                null,
                fileName);
        }

        string chapter = material.GetParameter(MaterialData.ChapterNameParameter);
        if(PythonFormat.Trim(chapter).Length > 0) {
            findings.Add(
                ReportSections.MaterialChapter,
                reference + ": глава заполнена («" + chapter
                + "»), для железобетона не заполняется (Стандарт п. 3.2.2.2).",
                null,
                fileName);
        }

        if(PythonFormat.Trim(material.GetParameter(MaterialData.WorkNameParameter)).Length > 0) {
            findings.Add(
                ReportSections.MaterialWork,
                reference + ": наименование работы заполнено, для железобетона не заполняется.",
                null,
                fileName);
        }

        string unit = material.GetParameter(MaterialData.UnitParameter);
        if(WorkClassifier.NormalizeUnit(unit) != "м³") {
            findings.Add(
                ReportSections.MaterialUnit,
                reference + ": единица измерения «" + (EmptyToNull(unit) ?? "не заполнена") + "», должна быть «м³».",
                null,
                fileName);
        }

        string countType = material.GetParameter(MaterialData.CountTypeParameter);
        if(PythonFormat.Trim(countType) != "3") {
            findings.Add(
                ReportSections.MaterialCountType,
                reference + ": тип подсчёта «" + (EmptyToNull(countType) ?? "не заполнен") + "», должен быть «3».",
                null,
                fileName);
        }
    }

    /// <summary>
    /// Прочие материалы ВОР: код из Классификатора, имя «[Код]_[Материал]_[Глава]_[Работа]», параметры по Классификатору.
    /// </summary>
    private static void CheckWorkMaterial(ModelCheckContext context, FindingCollection findings, MaterialData material) {
        string name = material.Name ?? "";
        string keynote = PythonFormat.Trim(material.Keynote);
        string reference = "Материал «" + name + "»";
        string fileName = context.FileName;
        context.Classifier.Works.TryGetValue(keynote, out ClassifierWork work);
        if(work == null) {
            findings.Add(
                ReportSections.MaterialKeynote,
                reference + ": код «" + keynote + "» отсутствует в Классификаторе видов работ проекта.",
                null,
                fileName);
        }

        if(!name.StartsWith(keynote + "_") || name.Count(character => character == '_') < 3) {
            findings.Add(
                ReportSections.MaterialName,
                reference + ": имя не соответствует схеме «[Код]_[Материал]_[Глава]_[Работа]» или не начинается "
                + "с кода ключевой пометки «" + keynote + "».",
                null,
                fileName);
        }

        if(_latinLetter.IsMatch(name.Split('_')[0])) {
            findings.Add(
                ReportSections.MaterialName,
                reference + ": в коде имени материала есть латинские буквы.",
                null,
                fileName);
        }

        if(PythonFormat.Trim(material.Description).Length == 0) {
            findings.Add(ReportSections.MaterialDescription, reference + ": «Описание» не заполнено.", null, fileName);
        }

        CheckChapter(context, findings, material, keynote, reference);
        if(work != null) {
            CheckWorkParameters(context, findings, material, keynote, reference, work);
        }
    }

    private static void CheckChapter(
        ModelCheckContext context,
        FindingCollection findings,
        MaterialData material,
        string keynote,
        string reference) {

        string chapterCode = keynote.Length >= 6 ? keynote.Substring(0, 6) : keynote;
        string expectedChapter =
            context.Classifier.Chapters.TryGetValue(chapterCode, out string classifierChapter)
            && !string.IsNullOrEmpty(classifierChapter)
                ? classifierChapter
                : _defaultChapters.TryGetValue(chapterCode, out string defaultChapter)
                    ? defaultChapter
                    : null;
        string chapter = PythonFormat.Trim(material.GetParameter(MaterialData.ChapterNameParameter));
        if(!string.IsNullOrEmpty(expectedChapter)
           && PythonFormat.NormalizeText(chapter) != PythonFormat.NormalizeText(expectedChapter)) {
            findings.Add(
                ReportSections.MaterialChapter,
                reference + ": глава «" + (chapter.Length > 0 ? chapter : "не заполнена") + "», по коду " + keynote
                + " должна быть «" + expectedChapter + "».",
                null,
                context.FileName);
        }
    }

    private static void CheckWorkParameters(
        ModelCheckContext context,
        FindingCollection findings,
        MaterialData material,
        string keynote,
        string reference,
        ClassifierWork work) {

        string fileName = context.FileName;
        string workName = PythonFormat.Trim(material.GetParameter(MaterialData.WorkNameParameter));
        if(PythonFormat.NormalizeText(workName) != PythonFormat.NormalizeText(work.Name)) {
            findings.Add(
                ReportSections.MaterialWork,
                reference + ": наименование работы «" + (workName.Length > 0 ? workName : "не заполнено")
                + "», по Классификатору для " + keynote + " — «" + work.Name + "».",
                null,
                fileName);
        }

        string unit = material.GetParameter(MaterialData.UnitParameter);
        if(WorkClassifier.NormalizeUnit(unit) != work.Unit) {
            findings.Add(
                ReportSections.MaterialUnit,
                reference + ": единица измерения «" + (EmptyToNull(unit) ?? "не заполнена")
                + "», по Классификатору — «" + work.Unit + "».",
                null,
                fileName);
        }

        string countType = PythonFormat.Trim(material.GetParameter(MaterialData.CountTypeParameter));
        string expectedCountType = _countTypeByUnit.TryGetValue(work.Unit, out string count) ? count : "?";
        if(countType != expectedCountType) {
            findings.Add(
                ReportSections.MaterialCountType,
                reference + ": тип подсчёта «" + (countType.Length > 0 ? countType : "не заполнен")
                + "», для единицы «" + work.Unit + "» должен быть «" + expectedCountType + "».",
                null,
                fileName);
        }
    }

    private static string EmptyToNull(string text) {
        return string.IsNullOrEmpty(text) ? null : text;
    }
}
