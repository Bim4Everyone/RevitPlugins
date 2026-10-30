using System.Text.RegularExpressions;

using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Раздел 6: ФОП_Этаж СМР соответствует положению элемента по высоте.
/// </summary>
/// <remarks>
/// Этаж элемента — уровень, на котором стоит низ элемента: ближайший уровень не выше точки
/// «низ + половина высоты, но не более 300 мм» (допуск на смещение низа ниже уровня у стен и пилонов).
/// Плита, верх которой на отметке уровня N, получает этаж N−1 — перекрытие над этажом относится к этому этажу.
/// Конструкция на несколько этажей получает этаж по низу.
/// В модели бывают уровни разных корпусов («02 этаж_К1_…», «02 этаж_К4_…»). Уровни элемента — уровни его корпуса
/// (корпус из имени уровня, на котором стоит элемент); если корпус не определён — уровни, в габарит которых
/// элемент попадает в плане; если и так остаются уровни нескольких корпусов, элемент не сверяется.
/// Сверяются только значения формата «NN этаж», для которых в модели есть уровень, — остальное уже в замечаниях выше.
/// </remarks>
internal static class FloorPositionRules {
    /// <summary>
    /// Насколько выше низа элемента берётся точка для поиска этажа, мм.
    /// </summary>
    private const double _maxProbeHeightMm = 300;

    /// <summary>
    /// Допуск попадания элемента в габарит уровня в плане, мм.
    /// </summary>
    private const double _planToleranceMm = 100;

    private static readonly Regex _floorName = new(@"^((К|П|Т|ТХ|Д)?\d{2}) этаж$");
    private static readonly Regex _firstFloorToken = new(@"^(К|П|Т|ТХ|Д)?\d{2}");

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        List<FloorLevel> levels = GetFloorLevels(context.Model.Levels);
        if(levels.Count == 0) {
            // уровни выгружены без отметок во внутренних координатах (старые данные) или названы не по Стандарту
            return;
        }

        var knownFloors = new HashSet<string>(levels.Select(level => level.FloorToken));
        var problems = new GroupedElements();
        foreach(ElementData element in context.VorElements) {
            string value = element.GetParameterOrEmpty(ModelCheckContext.FloorParameter).Trim();
            string valueToken = _firstFloorToken.Match(value).Value;
            if(valueToken.Length == 0 || !knownFloors.Contains(valueToken)) {
                continue;
            }

            FloorLevel? expected = FindFloor(element, SelectBuildingLevels(element, levels));
            if(expected == null || expected.FloorToken == valueToken) {
                continue;
            }

            problems.Add(
                "ФОП_Этаж СМР не соответствует положению элемента по высоте: этаж определяется по низу элемента "
                + "(перекрытие относится к этажу под ним, конструкция на несколько этажей — к нижнему). Проверить "
                + "значение параметра или отметку элемента.",
                "«" + value + "», по положению — «" + expected.FloorName + "» (уровень «" + expected.LevelName + "»)",
                element);
        }

        problems.AddFindings(findings, ReportSections.SmrFloor, context.FileName, 3, FindingKinds.Clarify);
    }

    /// <summary>
    /// Уровни с именем по Стандарту («NN этаж_…») и отметкой во внутренних координатах.
    /// </summary>
    private static List<FloorLevel> GetFloorLevels(IEnumerable<LevelData> levels) {
        var floorLevels = new List<FloorLevel>();
        foreach(LevelData level in levels) {
            Match floorName = _floorName.Match(level.Name.Split('_')[0]);
            if(!floorName.Success || level.ProjectElevation == null) {
                continue;
            }

            floorLevels.Add(new FloorLevel(level, floorName.Value, floorName.Groups[1].Value));
        }

        return floorLevels;
    }

    /// <summary>
    /// Уровни корпуса элемента; пустой список — корпус не определён однозначно.
    /// </summary>
    private static List<FloorLevel> SelectBuildingLevels(ElementData element, List<FloorLevel> levels) {
        string? building = GetBuildingCode(element.LevelName);
        if(building != null) {
            var buildingLevels = levels.Where(level => level.BuildingCode == building).ToList();
            if(buildingLevels.Count > 0) {
                return buildingLevels;
            }
        }

        List<double?>? box = element.BoundingBox;
        var candidates = levels;
        if(box?.Count >= 6 && box.All(value => value.HasValue)) {
            double centerX = (box[0].GetValueOrDefault() + box[3].GetValueOrDefault()) / 2;
            double centerY = (box[1].GetValueOrDefault() + box[4].GetValueOrDefault()) / 2;
            var levelsInPlan = levels.Where(level => level.Contains(centerX, centerY)).ToList();
            if(levelsInPlan.Count > 0) {
                candidates = levelsInPlan;
            }
        }

        bool isSingleBuilding = candidates
            .Select(level => level.BuildingCode)
            .Distinct()
            .Count() <= 1;
        return isSingleBuilding ? candidates : new List<FloorLevel>();
    }

    /// <summary>
    /// Корпус из имени уровня «13 этаж_К3_С4_38.600» → «К3»; null — имя не по Стандарту.
    /// </summary>
    private static string? GetBuildingCode(string? levelName) {
        string[] parts = (levelName ?? "").Split('_');
        return parts.Length >= 3 && _floorName.IsMatch(parts[0]) ? parts[1] : null;
    }

    /// <summary>
    /// Этаж, на котором стоит элемент; null — у элемента нет габарита или нет уровней.
    /// </summary>
    private static FloorLevel? FindFloor(ElementData element, List<FloorLevel> levels) {
        List<double?>? box = element.BoundingBox;
        if(levels.Count == 0 || box == null || box.Count < 6 || box.Any(value => !value.HasValue)) {
            return null;
        }

        double bottom = box[2].GetValueOrDefault();
        double height = box[5].GetValueOrDefault() - bottom;
        double probeZ = bottom + Math.Min(_maxProbeHeightMm, height / 2);
        var levelsInPlan = levels;

        FloorLevel levelBelow = levelsInPlan
            .Where(level => level.Elevation <= probeZ)
            .OrderByDescending(level => level.Elevation)
            .FirstOrDefault();

        // ниже всех уровней (фундаментная плита, подготовка) — ближайший по высоте уровень
        return levelBelow ?? levelsInPlan.OrderBy(level => Math.Abs(level.Elevation - probeZ)).First();
    }

    /// <summary>
    /// Уровень этажа: отметка, габарит в плане и этаж из имени уровня.
    /// </summary>
    private class FloorLevel {
        private readonly List<double>? _planExtent;

        public FloorLevel(LevelData level, string floorName, string floorToken) {
            LevelName = level.Name;
            Elevation = level.ProjectElevation.GetValueOrDefault();
            FloorName = floorName;
            FloorToken = floorToken;
            BuildingCode = GetBuildingCode(level.Name);
            _planExtent = level.PlanExtent?.Count == 4 ? level.PlanExtent : null;
        }

        public string LevelName { get; }

        public double Elevation { get; }

        /// <summary>
        /// Этаж из имени уровня, например «13 этаж».
        /// </summary>
        public string FloorName { get; }

        /// <summary>
        /// Обозначение этажа без слова «этаж», например «13», «П01».
        /// </summary>
        public string FloorToken { get; }

        /// <summary>
        /// Корпус из имени уровня, например «К3»; null — в имени нет корпуса.
        /// </summary>
        public string? BuildingCode { get; }

        /// <summary>
        /// Точка попадает в габарит уровня в плане; уровень без габарита подходит для любой точки.
        /// </summary>
        public bool Contains(double x, double y) {
            if(_planExtent == null) {
                return true;
            }

            return x >= _planExtent[0] - _planToleranceMm
                   && y >= _planExtent[1] - _planToleranceMm
                   && x <= _planExtent[2] + _planToleranceMm
                   && y <= _planExtent[3] + _planToleranceMm;
        }
    }
}
