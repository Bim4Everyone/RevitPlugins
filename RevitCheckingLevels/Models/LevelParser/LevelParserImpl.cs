using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RevitCheckingLevels.Models.LevelParser {
    internal class LevelParserImpl {
        public static readonly CultureInfo CultureInfo = GetCultureInfo();

        private readonly List<string> _errors = new List<string>();

        public LevelParserImpl(string levelName) {
            LevelName = levelName;
        }

        public string LevelName { get; }
        public IReadOnlyCollection<string> Errors => _errors;

        public LevelInfo ReadLevelInfo() {
            string[] splittedName = LevelName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            var levelInfo = new LevelInfo();
            ReadLevelName(levelInfo, splittedName[0]);
            ReadBlockName(levelInfo, splittedName[1]);
            ReadElevation(levelInfo, splittedName[2]);

            return levelInfo;
        }

        private void ReadLevelName(LevelInfo levelInfo, string levelName) {
            var splittedLevelName = levelName.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            if(splittedLevelName.Length == 2) {
                (string levelType, string levelNum) = ReadPrefix(splittedLevelName[0]);
                if(!splittedLevelName[1].Equals("этаж")) {
                    _errors.Add("После уровня должна идти надпись \"этаж\".");
                }

                levelInfo.Level = ReadInt32(levelNum, "Не удалось прочитать номер этажа.");
                levelInfo.LevelType = ReadPrefix<LevelType>(levelType, "Не удалось прочитать тип уровня.");
            } else {
                _errors.Add("Не удалось прочитать уровень.");
            }
        }

        private void ReadBlockName(LevelInfo levelInfo, string blockName) {
            string[] splittedBlockName = blockName.Split(new[] { '.' },
                StringSplitOptions.RemoveEmptyEntries);

            if(splittedBlockName.Length == 1) {
                var blockRange = splittedBlockName[0].Split(new[] { ',', '-' },
                    StringSplitOptions.RemoveEmptyEntries);

                if(blockRange.Length == 1) {
                    ReadBlock(levelInfo, splittedBlockName[0]);
                } else if(blockRange.Length == 2) {
                    ReadBlockRange(levelInfo, blockRange);
                } else {
                    _errors.Add("Не удалось прочитать диапазон типа блока.");
                }

            } else if(splittedBlockName.Length == 2) {
                ReadBlock(levelInfo, splittedBlockName[0]);
                levelInfo.SubLevel = ReadInt32(splittedBlockName[1], "Не удалось прочитать значение номера уровня.");
            } else {

                _errors.Add("Не удалось прочитать тип блока.");
            }

        }

        private void ReadElevation(LevelInfo levelInfo, string elevation) {
            if(double.TryParse(elevation, NumberStyles.AllowDecimalPoint, CultureInfo, out double result)) {
                levelInfo.Elevation = result;
                return;
            }

            _errors.Add("Не удалось прочитать отметку уровня.");
        }

        private void ReadBlock(LevelInfo levelInfo, string blockName) {
            (string blockType, string blockNum) = ReadPrefix(blockName);

            levelInfo.StartBlock = ReadInt32(blockNum, "Не удалось прочитать номер блока.");
            levelInfo.FinishBlock = levelInfo.StartBlock;
            levelInfo.BlockType = ReadPrefix<BlockType>(blockType, "Не удалось прочитать тип блока.");
        }

        private void ReadBlockRange(LevelInfo levelInfo, string[] blockRange) {
            (string startBlockType, string startBlockNum) = ReadPrefix(blockRange[0]);
            (string finishBlockType, string finishBlockNum) = ReadPrefix(blockRange[1]);

            if(!startBlockType.Equals(finishBlockType)) {
                _errors.Add("Наименование типов блоков не совпадают.");
            }

            levelInfo.BlockType = ReadPrefix<BlockType>(startBlockType, "Не удалось прочитать тип блока.");
            levelInfo.StartBlock = ReadInt32(startBlockNum, "Не удалось прочитать начальный номер блока.");
            levelInfo.FinishBlock = ReadInt32(finishBlockNum, "Не удалось прочитать конечный номер блока.");

            if(levelInfo.StartBlock > levelInfo.FinishBlock) {
                _errors.Add("Значение начального номера блока больше значения конечного блока.");
            }
        }

        private (string prefixName, string prefixNum) ReadPrefix(string prefixName) {
            var match = Regex.Match(prefixName, "^(?'type'[A-Za-zА-Яа-я]+)?(?'num'\\d{2})$");
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