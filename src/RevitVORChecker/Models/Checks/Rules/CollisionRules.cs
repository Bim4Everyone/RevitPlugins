using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Раздел 1 «Проверка на коллизии»: дубли и пересечения конструкций.
/// </summary>
internal static class CollisionRules {
    public static void Check(ModelCheckContext context, FindingCollection findings) {
        ClashesData clashes = context.Clashes;
        if(clashes == null) {
            return;
        }

        string fileName = context.FileName;
        AddDuplicates(context, findings, clashes, fileName);
        AddClashes(context, findings, clashes, fileName);
        if(clashes.GeometryErrorCount != 0) {
            findings.Add(
                ReportSections.Collisions,
                "Часть пар элементов не удалось проанализировать средствами Revit — рекомендуется проверка в Navisworks.",
                null,
                fileName,
                FindingKinds.Info,
                "пар: " + clashes.GeometryErrorCount);
        }
    }

    private static bool IsExcluded(ModelCheckContext context, long firstId, long secondId) {
        return context.ExcludedElementIds.Contains(firstId) || context.ExcludedElementIds.Contains(secondId);
    }

    private static void AddDuplicates(
        ModelCheckContext context,
        FindingCollection findings,
        ClashesData clashes,
        string fileName) {

        var duplicatesByType = new OrderedMap<string, List<DuplicateData>>();
        foreach(DuplicateData duplicate in clashes.Duplicates) {
            if(IsExcluded(context, duplicate.FirstId, duplicate.SecondId)) {
                continue;
            }

            duplicatesByType.GetOrAdd(duplicate.TypeName ?? "", () => new List<DuplicateData>()).Add(duplicate);
        }

        foreach(var typeDuplicates in duplicatesByType) {
            List<DuplicateData> pairs = typeDuplicates.Value;
            foreach(DuplicateData pair in pairs) {
                context.CollisionElementIds.Add(pair.FirstId);
                context.CollisionElementIds.Add(pair.SecondId);
            }

            findings.Add(
                ReportSections.Collisions,
                "Дублирование элементов: два одинаковых элемента в одном месте — двойной объём в ВОР.",
                pairs.Select(pair => pair.FirstId).Concat(pairs.Select(pair => pair.SecondId)),
                fileName,
                detail: typeDuplicates.Key + ", пар: " + pairs.Count);
        }
    }

    private static void AddClashes(
        ModelCheckContext context,
        FindingCollection findings,
        ClashesData clashes,
        string fileName) {

        string tolerance = PythonFormat.NumberText(clashes.ToleranceMm);
        int volumeDecimals = context.Project.ClashVolumeDecimals;
        var clashesByTypes = new OrderedMap<string, List<ClashData>>();
        var typePairs = new Dictionary<string, string[]>();
        foreach(ClashData clash in clashes.Clashes) {
            if(PythonFormat.Round(clash.VolumeM3, volumeDecimals) == 0) {
                continue;
            }

            if(IsExcluded(context, clash.FirstId, clash.SecondId)) {
                continue;
            }

            string[] pair = new[] { clash.FirstTitle.Split(':')[0], clash.SecondTitle.Split(':')[0] }
                .OrderBy(title => title, StringComparer.Ordinal)
                .ToArray();
            string key = pair[0] + "\u0001" + pair[1];
            typePairs[key] = pair;
            clashesByTypes.GetOrAdd(key, () => new List<ClashData>()).Add(clash);
        }

        foreach(var group in clashesByTypes) {
            string firstCategory = typePairs[group.Key][0];
            string secondCategory = typePairs[group.Key][1];
            List<ClashData> pairs = group.Value;
            string what = firstCategory == secondCategory
                ? "«" + firstCategory + "» между собой"
                : "«" + firstCategory + "» и «" + secondCategory + "»";
            double totalVolume = pairs.Sum(pair => pair.VolumeM3);
            foreach(ClashData pair in pairs) {
                context.CollisionElementIds.Add(pair.FirstId);
                context.CollisionElementIds.Add(pair.SecondId);
            }

            string pairIds = string.Join("; ", pairs.Select(pair => pair.FirstId + ", " + pair.SecondId));
            findings.Add(
                ReportSections.Collisions,
                "Пересечение конструкций " + what + " глубиной не менее " + tolerance
                + " мм — двойной учёт объёма бетона в ВОР. Проверить соединение (Join) и примыкание "
                + "по статье «КР. Мод. Опалубка и отверстия».",
                pairs.Select(pair => pair.FirstId).Concat(pairs.Select(pair => pair.SecondId)),
                fileName,
                detail: "пар: " + pairs.Count + ", объём пересечений ≈ " + PythonFormat.FormatFixed(totalVolume, 2)
                        + " м³. Пары ID: " + pairIds);
        }
    }
}
