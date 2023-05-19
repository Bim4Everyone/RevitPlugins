using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

namespace RevitCheckingLevels.Models.LevelParser {
    internal class LevelParserImpl {
        private readonly Level _level;
        public static readonly CultureInfo CultureInfo = GetCultureInfo();

        private readonly List<string> _errors = new List<string>();
        private readonly List<ILevelBlock> _levelBlocks = new List<ILevelBlock>();

        public LevelParserImpl(Level level) {
            _level = level;
        }

        public Level Level => _level;
        public string LevelName => _level.Name;

        public LevelInfo ReadLevelInfo() {
            string[] splittedName = LevelName.Split(
                new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            var levelInfo = new LevelInfo { Level = _level, Errors = _errors, LevelBlocks = _levelBlocks};
            ReadLevelName(levelInfo, splittedName.ElementAtOrDefault(0));
            ReadBlockName(levelInfo, splittedName.ElementAtOrDefault(1));
            ReadElevation(levelInfo, splittedName.ElementAtOrDefault(2));
            
            return levelInfo;
        }

        private void ReadLevelName(LevelInfo levelInfo, string levelName) {
            levelInfo.LevelName = levelName;
            if(levelName == null) {
                _errors.Add("Не удалось прочитать уровень.");
                return;
            }

            var splittedLevelName = levelName.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            if(splittedLevelName.Length == 2) {
                (string levelType, string levelNum) = ReadPrefix(splittedLevelName[0]);
                if(!splittedLevelName[1].Equals("этаж")) {
                    _errors.Add("После уровня должна идти надпись \"этаж\".");
                }

                levelInfo.LevelNum = ReadInt32(levelNum, "Не удалось прочитать номер этажа.");
                levelInfo.LevelType = ReadPrefix<LevelType>(levelType, "Не удалось прочитать тип уровня.");
            } else {
                _errors.Add("Не удалось прочитать уровень.");
            }
        }

        private void ReadBlockName(LevelInfo levelInfo, string blockName) {
            levelInfo.BlockName = blockName;
            if(blockName == null) {
                _errors.Add("Не удалось прочитать тип блока.");
                return;
            }

            if(blockName.Contains('.')) {
                _levelBlocks.Add(ReadBlock(blockName));
            } else {
                var blockRange = blockName.Split(new[] {','},
                    StringSplitOptions.RemoveEmptyEntries);

                foreach(string block in blockRange) {
                    if(block.Contains('-')) {
                        var levelBlockRange = ReadBlockRange(blockName);
                        if(!levelBlockRange.StartBlock.BlockType
                               .Equals(levelBlockRange.FinishBlock.BlockType)) {
                            _errors.Add("Наименование типов блоков не совпадают.");
                        }

                        if(levelBlockRange.StartBlock.BlockNum > levelBlockRange.FinishBlock.BlockNum) {
                            _errors.Add("Значение начального номера блока больше значения конечного блока.");
                        }
                        
                        if(levelBlockRange.StartBlock.BlockNum == levelBlockRange.FinishBlock.BlockNum) {
                            _errors.Add("Значение начального номера блока и значения конечного блока равны.");
                        }
                    } else {
                        _levelBlocks.Add(ReadBlock(blockName));
                    }
                }
            }
        }

        private void ReadElevation(LevelInfo levelInfo, string elevation) {
            levelInfo.ElevationName = elevation;
            if(elevation == null) {
                _errors.Add("Не удалось прочитать отметку уровня.");
                return;
            }

            if(!Regex.IsMatch(elevation, @"-?\d+\.\d\d\d")) {
                _errors.Add("Отметка уровня не верного формата.");
                return;
            }

            if(double.TryParse(elevation, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo, out double result)) {
                levelInfo.Elevation = result;
                return;
            }

            _errors.Add("Не удалось прочитать отметку уровня.");
        }

        private LevelBlock ReadBlock(string blockName) {
            (string blockType, string blockNum) = ReadPrefix(blockName);

            int? subLevel = blockName.Contains('.')
                ? ReadInt32(blockName.Split('.').LastOrDefault(), "Не удалось прочитать значение номера уровня.")
                : (int?) null;

            return new LevelBlock() {
                SubLevel = subLevel,
                BlockNum = ReadInt32(blockNum, null),
                BlockType = ReadPrefix<BlockType>(blockType, "Не удалось прочитать тип блока.")
            };
        }

        private LevelBlockRange ReadBlockRange(string blockName) {
            string[] blockRange = blockName.Split('-');
            return new LevelBlockRange() {
                StartBlock = ReadBlock(blockRange[0]),
                FinishBlock = ReadBlock(blockRange[1])
            };
        }

        private (string prefixName, string prefixNum) ReadPrefix(string prefixName) {
            var match = Regex.Match(prefixName, "^(?'type'[A-Za-zА-Яа-я]+)?(?'num'\\d{1,2})?");
            return (match.Groups["type"].Value, match.Groups["num"].Value);
        }

        private T ReadPrefix<T>(string prefixName, string errorText)
            where T : PrefixName {
            var prefix = PrefixName.GetPrefixByName<T>(prefixName);
            if(prefix == null) {
                _errors.Add(errorText);
            }

            return prefix;
        }

        private int ReadInt32(string value, string errorText) {
            if(!int.TryParse(value, out int result)) {
                if(errorText == null) {
                    return 1;
                }
                _errors.Add(errorText);
            }

            return result;
        }

        private static CultureInfo GetCultureInfo() {
            var cultureInfo = (CultureInfo) CultureInfo.GetCultureInfo("ru-Ru").Clone();
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            return cultureInfo;
        }
    }
}