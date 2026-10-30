using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Report;

/// <summary>
/// Задания на скриншоты: для записей замечаний с более чем 10 элементами и для коллизий (по первой паре).
/// </summary>
internal static class ScreenshotPlanner {
    private const int _maxListedIds = 10;
    private const int _maxIdsPerScreenshot = 2000;

    /// <summary>
    /// Возвращает задания по файлам моделей и проставляет записям замечаний путь к будущему скриншоту.
    /// </summary>
    public static OrderedMap<string, List<ScreenshotJob>> Plan(
        List<Finding> findings,
        string imageFolder,
        IEnumerable<string> modelFiles) {

        Dictionary<long, ElementData> elementsById = LoadElements(modelFiles);
        var jobsByFile = new OrderedMap<string, List<ScreenshotJob>>();
        for(int findingNumber = 1; findingNumber <= findings.Count; findingNumber++) {
            Finding finding = findings[findingNumber - 1];
            for(int entryNumber = 1; entryNumber <= finding.Entries.Count; entryNumber++) {
                FindingEntry entry = finding.Entries[entryNumber - 1];
                if(entry.ElementIds.Count == 0 || entry.FileName is not { Length: > 0 } fileName) {
                    continue;
                }

                string name = finding.Section + "_" + findingNumber.ToString("000") + "_" + entryNumber.ToString("00");
                ScreenshotJob? job = CreateJob(finding, entry, name, elementsById);
                if(job == null) {
                    continue;
                }

                jobsByFile.GetOrAdd(fileName, () => new List<ScreenshotJob>()).Add(job);
                entry.ImagePath = Path.Combine(imageFolder, name + ".png");
            }
        }

        var sortedJobs = new OrderedMap<string, List<ScreenshotJob>>();
        foreach(string fileName in jobsByFile.Keys.OrderBy(name => name, StringComparer.Ordinal)) {
            sortedJobs[fileName] = jobsByFile[fileName];
        }

        return sortedJobs;
    }

    private static ScreenshotJob? CreateJob(
        Finding finding,
        FindingEntry entry,
        string name,
        Dictionary<long, ElementData> elementsById) {

        Match firstPair = Regex.Match(entry.Detail ?? "", @"Пары ID: (\d+), (\d+)");
        if(!firstPair.Success) {
            firstPair = Regex.Match(entry.Detail ?? "", @"пары ID (\d+) × (\d+)");
        }

        if(finding.Section == ReportSections.Collisions && firstPair.Success) {
            var pairIds = new List<long> { long.Parse(firstPair.Groups[1].Value), long.Parse(firstPair.Groups[2].Value) };
            return new ScreenshotJob(name, pairIds, ScreenshotFraming.Intersection);
        }

        if(entry.ElementIds.Count > _maxListedIds) {
            ScreenshotFraming framing = AreSmallAndScattered(entry.ElementIds, elementsById)
                ? ScreenshotFraming.Fragment
                : ScreenshotFraming.Union;
            return new ScreenshotJob(name, entry.ElementIds.Take(_maxIdsPerScreenshot).ToList(), framing);
        }

        return null;
    }

    private static Dictionary<long, ElementData> LoadElements(IEnumerable<string> modelFiles) {
        var elementsById = new Dictionary<long, ElementData>();
        foreach(string modelFile in modelFiles) {
            foreach(ElementData element in JsonFiles.Load<ModelData>(modelFile).Elements) {
                if(element.BoundingBox?.Count >= 6 && element.BoundingBox.All(value => value.HasValue)) {
                    elementsById[element.Id] = element;
                }
            }
        }

        return elementsById;
    }

    /// <summary>
    /// Мелкие элементы, разбросанные по модели: на общем виде их не различить.
    /// </summary>
    private static bool AreSmallAndScattered(List<long> elementIds, Dictionary<long, ElementData> elementsById) {
        var elements = elementIds.Where(elementsById.ContainsKey).Select(id => elementsById[id]).ToList();
        if(elements.Count < 2) {
            return false;
        }

        var boxes = elements.Select(element => element.BoundingBox!.Select(value => value.GetValueOrDefault()).ToArray()).ToList();
        var volumes = elements.Select(element => element.Volume ?? 0).OrderBy(volume => volume).ToList();
        var diagonals = boxes.Select(box => Distance(box, 0, box, 3)).OrderBy(diagonal => diagonal).ToList();
        double[] minimum = Enumerable.Range(0, 3).Select(axis => boxes.Min(box => box[axis])).ToArray();
        double[] maximum = Enumerable.Range(0, 3).Select(axis => boxes.Max(box => box[axis + 3])).ToArray();
        return volumes[(int) (volumes.Count * 0.9)] < 0.25
               && diagonals[diagonals.Count / 2] * 25 < Distance(minimum, 0, maximum, 0);
    }

    private static double Distance(double[] first, int firstOffset, double[] second, int secondOffset) {
        return Math.Sqrt(Enumerable.Range(0, 3).Sum(axis => {
            double difference = first[firstOffset + axis] - second[secondOffset + axis];
            return difference * difference;
        }));
    }
}
