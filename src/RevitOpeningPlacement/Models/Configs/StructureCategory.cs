using System.Linq;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs;
internal class StructureCategory {
    public bool IsSelected { get; set; }
    public string Name { get; set; }
    /// <summary>
    /// Правила фильтрации элементов данной категории
    /// </summary>
    public Set Set { get; set; } = new Set() {
        SetEvaluator = SetEvaluatorUtils.GetEvaluators().First(),
        Criteria = []
    };
}
