using System.Text.RegularExpressions;

namespace RevitVORChecker.Models.Project;

/// <summary>
/// Нормы армирования по табл. 2.6.4 ЗнП А101 и построение из них правил проверки ФОП_ТИП_Армирование.
/// </summary>
internal static class ReinforcementNorms {
    private const string Parking = "Автостоянка";

    private static readonly string[] _slabKeys = { "Перекрытие", "Покрытие", "Капитель", "Балка", "Бортик", "Рампа" };
    private static readonly string[] _innerWallPurposes = { "Внутренние", "Диафрагма", "ЛЛБ" };

    /// <summary>
    /// Типовая табл. 2.6.4 ЗнП А101 (строки и значения — как в ЗнП).
    /// </summary>
    public static List<ReinforcementTableRow> CreateDefaultTable() {
        return new List<ReinforcementTableRow> {
            new("K1", "1", "Фундаментная плита для 9-этажных домов", "Табл. 2.6.2", 110, null),
            new("K2", "2", "Фундаментная плита для 17-этажных домов", "Табл. 2.6.2", null, 120),
            new("K3", "3", "Подземная часть. Стены наружные (между пилонами)", "200 мм", 180, 180),
            new("K4", "4", "Подземная часть. Стены внутренние (ЛЛУ и диафрагмы жесткости) для 9-этажных домов",
                "180 мм", 130, null),
            new("K5", "5", "Подземная часть. Стены внутренние (ЛЛУ и диафрагмы жесткости) для 17-этажных домов",
                "200 мм", null, 130),
            new("K6", "6", "Надземная часть. Стены внутренние (ЛЛУ и диафрагмы жесткости) для 9-этажных домов",
                "180 мм — для первого этажа, 160 мм — для типового этажа", 120, null),
            new("K7", "7", "Надземная часть. Стены внутренние (ЛЛУ и диафрагмы жесткости) для 17-этажных домов",
                "200 мм — для первого этажа, 180 мм — для типового этажа", null, 120),
            new("K8", "8", "Подземная часть. Пилоны",
                "Центральные — 300 мм, в створе наружных стен — 250 мм", 220, 250),
            new("K9", "9", "Надземная часть. Пилоны типового этажа",
                "250 мм для 1 этажа, 200 мм для типового этажа", 170, 250),
            new("K10", "10", "Плита перекрытия на 0,000", "240 мм", 120, 120),
            new("K11", "11", "Плиты перекрытий типовых этажей и покрытие", "200 мм", 130, 130),
            new("P1", "1", "Фундаментная плита (без армирования пилонов)", "450 мм", 110, null),
            new("P2", "2", "Плита покрытия паркинга", "500 мм — опорная часть, 250 мм — пролетная часть", 200, 220,
                "для покрытий сложной формы с большим количеством перепадов"),
            new("P3", "3", "Стены наружные", "200 мм", 180, 200, "при высоте паркинга более 2,9 м"),
            new("P4", "4", "Стены внутренние (диафрагмы жесткости)", "200 мм", 130, null),
            new("P5", "5", "Пилоны", "300 мм", 370, null),
            new("P6", "6", "Плита перекрытия (2-уровневый паркинг)",
                "200–220 мм (уточняется расчётом на продавливание)", 130, null)
        };
    }

    /// <summary>
    /// Правила проверки по типовой таблице.
    /// </summary>
    public static List<ReinforcementRule> CreateDefaultRules(StoreysOption storeys) {
        return CreateRules(CreateDefaultTable(), storeys, null);
    }

    /// <summary>
    /// Правила проверки армирования из табл. 2.6.4.
    /// </summary>
    /// <remarks>
    /// Для корпусов берётся столбец по этажности: для 18+ этажей — столбец 10–17, как предписывает ЗнП.
    /// Из пар строк «для 9-/17-этажных домов» берётся та, где столбец заполнен.
    /// При разной этажности корпуса из <paramref name="lowRiseBlocks"/> проверяются по столбцу 7–9, остальные — по 10–17.
    /// </remarks>
    public static List<ReinforcementRule> CreateRules(
        List<ReinforcementTableRow> table,
        StoreysOption storeys,
        IList<string> lowRiseBlocks) {

        var builder = new RulesBuilder(table);
        builder.AddParkingRules();
        if(storeys == StoreysOption.Mixed) {
            string[] lowBlocks = (lowRiseBlocks ?? Array.Empty<string>())
                .Where(block => !string.IsNullOrWhiteSpace(block))
                .Select(block => block.Trim())
                .ToArray();
            if(lowBlocks.Length > 0) {
                builder.AddResidentialRules(true, lowBlocks, "7–9 эт.", false);
            }

            // правила применяются сверху вниз — сначала перечисленные корпуса до 9 этажей
            builder.AddResidentialRules(false, new[] { "Корпус" }, "10–17 эт.", false);
        } else {
            string columnTitle = storeys switch {
                StoreysOption.UpTo9 => "7–9 эт.",
                StoreysOption.From10To17 => "10–17 эт.",
                _ => "18+ эт.: по столбцу 10–17, вертикали — в первом приближении"
            };
            builder.AddResidentialRules(
                storeys == StoreysOption.UpTo9,
                new[] { "Корпус" },
                columnTitle,
                storeys == StoreysOption.From18);
        }

        return builder.Rules;
    }

    /// <summary>
    /// Таблица для окна «Данные проекта».
    /// </summary>
    /// <remarks>
    /// Берётся сохранённая таблица; для старых данных (только правила) — типовая таблица,
    /// в которой столбец этажности проекта заполнен нормами из правил (по пометке «Корпуса п.N» / «Паркинг п.N»).
    /// </remarks>
    public static List<ReinforcementTableRow> CreateTable(ProjectCard card, StoreysOption storeys) {
        List<ReinforcementTableRow> table = CreateDefaultTable();
        if(card.ReinforcementTable.Count > 0) {
            foreach(ReinforcementTableValue saved in card.ReinforcementTable) {
                ReinforcementTableRow row = table.FirstOrDefault(item => item.Key == saved.Key);
                if(row == null) {
                    continue;
                }

                row.Thickness = saved.Thickness ?? row.Thickness;
                row.FirstValue = saved.FirstValue;
                row.SecondValue = saved.SecondValue;
            }

            return table;
        }

        foreach(ReinforcementRule rule in card.ReinforcementRules) {
            Match match = Regex.Match(rule.Note ?? "", @"^(Корпуса|Паркинг) п\.(\d+)");
            if(!match.Success) {
                continue;
            }

            string key = (match.Groups[1].Value == "Паркинг" ? "P" : "K") + match.Groups[2].Value;
            ReinforcementTableRow row = table.FirstOrDefault(item => item.Key == key);
            if(row == null) {
                continue;
            }

            int value = (int) rule.Value;
            bool listsLowRiseBlocks = rule.Blocks != null && rule.Blocks.Any(block => block != "Корпус");
            if(row.IsParking) {
                row.FirstValue = value;
                row.SecondValue = rule.AlternativeValues?.Count > 0 ? (int) rule.AlternativeValues[0] : null;
            } else if(storeys == StoreysOption.UpTo9 || (storeys == StoreysOption.Mixed && listsLowRiseBlocks)) {
                row.FirstValue = value;
            } else {
                row.SecondValue = value;
            }
        }

        return table;
    }

    public static List<ReinforcementTableValue> ToTableValues(IEnumerable<ReinforcementTableRow> table) {
        return table
            .Select(row => new ReinforcementTableValue {
                Key = row.Key,
                Thickness = row.Thickness,
                FirstValue = row.FirstValue,
                SecondValue = row.SecondValue
            })
            .ToList();
    }

    /// <summary>
    /// Построитель списка правил из строк таблицы.
    /// </summary>
    private class RulesBuilder {
        private readonly List<ReinforcementTableRow> _table;

        public RulesBuilder(List<ReinforcementTableRow> table) {
            _table = table;
        }

        public List<ReinforcementRule> Rules { get; } = new();

        public void AddParkingRules() {
            string[] parking = { Parking };
            ReinforcementTableRow row = FindRow("P1");
            Add(parking, null, new[] { "ФПлита" }, null, null, row?.FirstValue, AlternativeOf(row),
                "Паркинг п.1 — фундаментная плита");

            row = FindRow("P2");
            Add(parking, null, _slabKeys, null, new[] { "П01 этаж" }, row?.FirstValue, AlternativeOf(row),
                "Паркинг п.2 — плита покрытия паркинга (балки, бортики, капители — как плита)");
            Add(parking, null, new[] { "Парапет" }, null, null, row?.FirstValue, AlternativeOf(row),
                "Паркинг п.2 — парапет как плита покрытия");

            row = FindRow("P6");
            Add(parking, null, _slabKeys, null, null, row?.FirstValue, AlternativeOf(row),
                "Паркинг п.6 — плита перекрытия 2-уровневого паркинга");

            row = FindRow("P3");
            string highParkingNote = row?.SecondValue != null
                ? " (" + row.SecondValue + " при высоте паркинга > 2,9 м)"
                : "";
            Add(parking, null, new[] { "Стена" }, new[] { "Наружные" }, null, row?.FirstValue, AlternativeOf(row),
                "Паркинг п.3 — стены наружные" + highParkingNote);

            row = FindRow("P4");
            Add(parking, null, new[] { "Стена" }, new[] { "Внутренние", "Диафрагма" }, null, row?.FirstValue,
                AlternativeOf(row), "Паркинг п.4 — стены внутренние (диафрагмы)");

            row = FindRow("P5");
            Add(parking, null, new[] { "Пилон", "Колонна" }, null, null, row?.FirstValue, AlternativeOf(row),
                "Паркинг п.5 — пилоны");
        }

        /// <summary>
        /// Правила для жилых корпусов по столбцу 7–9 (<paramref name="isLowRise"/>) или 10–17.
        /// </summary>
        public void AddResidentialRules(bool isLowRise, string[] blocks, string columnTitle, bool isEighteenPlus) {
            int? ColumnValue(ReinforcementTableRow row) {
                return row == null ? null : isLowRise ? row.FirstValue : row.SecondValue;
            }

            ReinforcementTableRow FirstFilled(params string[] keys) {
                return keys.Select(FindRow).FirstOrDefault(row => ColumnValue(row).HasValue);
            }

            ReinforcementTableRow row = FirstFilled("K1", "K2");
            if(row != null) {
                string houseHeight = row.Key == "K1" ? "9" : "17";
                string eighteenNote = isEighteenPlus ? " — 18+ эт.: по столбцу 10–17" : "";
                Add(blocks, null, new[] { "ФПлита" }, null, null, ColumnValue(row), null,
                    $"Корпуса п.{row.Number} — фундаментная плита ({houseHeight}-эт. дома; "
                    + $"выпуски под стены и пилоны включены){eighteenNote}");
            }

            Add(blocks, new[] { "НН" }, new[] { "Стена" }, new[] { "Наружные" }, null, ColumnValue(FindRow("K3")), null,
                "Корпуса п.3 — подземная часть, стены наружные");

            row = FirstFilled("K4", "K5");
            if(row != null) {
                Add(blocks, new[] { "НН" }, new[] { "Стена", "Балка" }, _innerWallPurposes, null, ColumnValue(row), null,
                    $"Корпуса п.{row.Number} — подземная часть, стены внутренние (ЛЛУ, диафрагмы); балки ЛЛУ — как стены");
            }

            row = FirstFilled("K6", "K7");
            if(row != null) {
                Add(blocks, new[] { "ВН" }, new[] { "Стена", "Балка" }, _innerWallPurposes, null, ColumnValue(row), null,
                    $"Корпуса п.{row.Number} — надземная часть, стены внутренние (ЛЛУ, диафрагмы); балки ЛЛУ — как стены");
            }

            Add(blocks, new[] { "НН" }, new[] { "Пилон", "Колонна" }, null, null, ColumnValue(FindRow("K8")), null,
                $"Корпуса п.8 — подземная часть, пилоны ({columnTitle})");
            Add(blocks, new[] { "ВН" }, new[] { "Пилон", "Колонна" }, null, null, ColumnValue(FindRow("K9")), null,
                $"Корпуса п.9 — надземная часть, пилоны ({columnTitle})");
            Add(blocks, new[] { "НН" }, new[] { "Перекрытие", "Покрытие", "Капитель", "Балка" }, null, null,
                ColumnValue(FindRow("K10")), null,
                "Корпуса п.10 — плита перекрытия на 0,000 (240 мм); балки/капители — как плита");
            Add(blocks, new[] { "ВН" }, new[] { "Перекрытие", "Покрытие", "Капитель", "Балка", "Парапет" }, null, null,
                ColumnValue(FindRow("K11")), null,
                "Корпуса п.11 — плиты типовых этажей и покрытие; балки, капители, парапеты — как плита");
        }

        private ReinforcementTableRow FindRow(string key) {
            return _table.FirstOrDefault(row => row.Key == key);
        }

        private static int[] AlternativeOf(ReinforcementTableRow row) {
            return row?.SecondValue == null ? null : new[] { row.SecondValue.Value };
        }

        private void Add(
            string[] blocks,
            string[] cycles,
            string[] typeKeys,
            string[] purposes,
            string[] floors,
            int? value,
            int[] alternativeValues,
            string note) {

            if(!value.HasValue) {
                return;
            }

            Rules.Add(new ReinforcementRule {
                Blocks = blocks?.ToList(),
                Cycles = cycles?.ToList(),
                TypeKeys = typeKeys?.ToList(),
                Purposes = purposes?.ToList(),
                Floors = floors?.ToList(),
                Value = value.Value,
                AlternativeValues = alternativeValues?.Select(item => (double) item).ToList(),
                Note = note
            });
        }
    }
}
