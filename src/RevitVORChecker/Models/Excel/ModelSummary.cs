using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Checks;
using RevitVORChecker.Models.Data;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Сводка по модели для сверки ВОР: основной блок, подземные этажи, заполненность ФОП_ТИП_Доп. сортировка.
/// </summary>
internal class ModelSummary {
    public string FileTitle { get; private set; } = string.Empty;

    /// <summary>
    /// Самое частое значение ФОП_Блок СМР у элементов ВОР.
    /// </summary>
    public string Block { get; private set; } = string.Empty;

    public int UndergroundFloorCount { get; private set; }

    /// <summary>
    /// Элементов ВОР без ФОП_ТИП_Доп. сортировка.
    /// </summary>
    public int UnsortedElementCount { get; private set; }

    public int VorElementCount { get; private set; }

    public static List<ModelSummary> Load(IEnumerable<string> modelFiles) {
        return modelFiles.Select(file => Create(JsonFiles.Load<ModelData>(file))).ToList();
    }

    private static ModelSummary Create(ModelData model) {
        var materials = model.Materials.GroupBy(material => material.Id).ToDictionary(group => group.Key, group => group.Last());
        var blocks = new List<string>();
        int vorCount = 0;
        int unsortedCount = 0;
        foreach(ElementData element in model.Elements) {
            bool isVor = element.GetMaterialIds().Any(id =>
                materials.TryGetValue(id, out MaterialData material) && (material.Keynote ?? "").StartsWith("г02"));
            if(!isVor) {
                continue;
            }

            string? block = element.GetParameter(ModelCheckContext.BlockParameter);
            if(block is { Length: > 0 }) {
                blocks.Add(block);
            }

            vorCount++;
            if(string.IsNullOrEmpty(element.GetParameter(ModelCheckContext.AdditionalSortingParameter))) {
                unsortedCount++;
            }
        }

        var mostCommonBlocks = PythonFormat.MostCommon(blocks);
        return new ModelSummary {
            FileTitle = model.File.Title,
            Block = mostCommonBlocks.Count > 0 ? mostCommonBlocks[0].Key : "",
            UndergroundFloorCount = model.Levels
                .Select(level => level.Name)
                .Where(name => Regex.IsMatch(name, @"^П\d{2} этаж"))
                .Select(name => name.Split('_')[0])
                .Distinct()
                .Count(),
            UnsortedElementCount = unsortedCount,
            VorElementCount = vorCount
        };
    }
}
