using System.Globalization;

namespace RevitCheckingLevels.Models.LevelParser {
    internal class LevelBlock : ILevelBlock {
        /// <summary>
        /// Номер блока.
        /// </summary>
        public int BlockNum { get; set; }
        
        /// <summary>
        /// Номер уровня.
        /// </summary>
        public int? SubLevel { get; set; }

        /// <summary>
        /// Тип блока.
        /// </summary>
        public BlockType BlockType { get; set; }
        
        public bool HasSubLevel() {
            return SubLevel.HasValue;
        }

        public string FormatLevelBlockName(CultureInfo cultureInfo) {
            return SubLevel == null
                ? BlockType.Name + BlockNum
                : BlockType.Name + BlockNum + "." + SubLevel;
        }
    }

    internal class LevelBlockRange : ILevelBlock {
        /// <summary>
        /// Начальный блок уровня.
        /// </summary>
        public LevelBlock StartBlock { get; set; }
        
        /// <summary>
        /// Конечный блок уровня.
        /// </summary>
        public LevelBlock FinishBlock { get; set; }

        public bool HasSubLevel() {
            return false;
        }

        public string FormatLevelBlockName(CultureInfo cultureInfo) {
            return StartBlock.FormatLevelBlockName(cultureInfo)
                   + "-"
                   + FinishBlock.FormatLevelBlockName(cultureInfo);
        }
    }

    internal interface ILevelBlock {
        bool HasSubLevel();
        
        /// <summary>
        /// Форматирует наименование блока.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns>Возвращает форматирование имя блока.</returns>
        string FormatLevelBlockName(CultureInfo cultureInfo);
    }
}