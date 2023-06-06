using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCheckingLevels.Models.LevelParser {
    /// <summary>
    /// Информация об уровне.
    /// </summary>
    internal class LevelInfo {
        /// <summary>
        /// Разобранный уровень.
        /// </summary>
        public Level Level { get; set; }
        
        public string LevelName { get; set; }
        public string BlockName { get; set; }
        public string ElevationName { get; set; }

        public bool HasSubLevel() {
            return BlockName.Contains(".");
        }

        public bool IsEqualBlockName(LevelInfo levelInfo) {
            return levelInfo.BlockName.Equals(BlockName);
        }
        
        public bool IsEqualLevelName(LevelInfo levelInfo) {
            return levelInfo.LevelName.Equals(LevelName);
        }
        
        public bool IsEqualElevation(LevelInfo levelInfo) {
            return levelInfo.ElevationName.Equals(ElevationName);
        }

        /// <summary>
        /// Номер этажа.
        /// </summary>
        public int? LevelNum { get; set; }

        /// <summary>
        /// Тип этажа.
        /// </summary>
        public LevelType LevelType { get; set; }

        /// <summary>
        /// Блоки уровня.
        /// </summary>
        public IReadOnlyCollection<ILevelBlock> LevelBlocks { get; set; } = new List<ILevelBlock>();

        /// <summary>
        /// Отметка уровня.
        /// </summary>
        public double? Elevation { get; set; }

        /// <summary>
        /// Список ошибок при разборе имени.
        /// </summary>
        public IReadOnlyCollection<string> Errors { get; set; } = new List<string>();

        public string FormatLevelName() {
            return Errors.Count > 0
                ? Level.Name
                : $"{GetLevelName()}_{GetBlockName()}_{GetElevation()}";
        }

        public string GetLevelName() {
            return $"{LevelType?.Name}{LevelNum:D2} этаж";
        }

        public string GetBlockName() {
            return string.Join(", ", LevelBlocks);
        }

        public string GetElevation() {
            return FormatElevation(Elevation);
        }
        
        public string FormatElevation(double? elevation) {
            return elevation?.ToString("F3", LevelParserImpl.CultureInfo);
        }

        public override string ToString() {
            return $"{GetLevelName()}_{GetBlockName()}_{GetElevation()}";
        }
    }
}