using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitCheckingLevels.Models.LevelParser;

namespace RevitCheckingLevels.Models {
    internal class ErrorType : IEquatable<ErrorType>, IComparable<ErrorType>, IComparable {
        public static readonly ErrorType NotStandard =
            new ErrorType(0) {
                Name = "Имена уровней не соответствуют стандарту",
                Description =
                    "Имена уровней должны соответствовать данному формату: \"[Префикс][Номер этажа] [пробел] [«этаж»][\"_\"][Название блока][\".\"][Номер уровня][\"_\"][Отметка уровня]\"."
            };

        public static readonly ErrorType NotElevation =
            new ErrorType(1) {
                Name = "Отметки уровня не соответствуют фактическим",
                Description =
                    $"Имена уровней должны оканчиваться значением параметра \"{LabelUtils.GetLabelFor(BuiltInParameter.LEVEL_ELEV)}\" в метрах с разделителем дробной части в виде точки."
            };

        public static readonly ErrorType NotMillimeterElevation =
            new ErrorType(2) {
                Name = "Отметка уровня не округлена",
                Description =
                    $"Значение параметра \"{LabelUtils.GetLabelFor(BuiltInParameter.LEVEL_ELEV)}\" (в миллиметрах) до 7 знака после запятой должно быть равно \"0\"."
            };

        public static readonly ErrorType NotRangeElevation =
            new ErrorType(3) {
                Name = "Уровни замоделированы не по стандарту",
                Description =
                    $"Значение параметра \"{LabelUtils.GetLabelFor(BuiltInParameter.LEVEL_ELEV)}\" должно отступать на 1500мм от предыдущего."
            };

        public static readonly ErrorType NotFoundLevels =
            new ErrorType(4) {
                Name = "Не были найдены уровни в открытом проекте",
                Description = "Все уровни должны быть скопированы с координационного файла."
            };

        public static readonly ErrorType NotFoundLinkLevels =
            new ErrorType(5) {
                Name = "Не были найдены уровни в координационном файле",
                Description = "Все уровни должны быть скопированы с координационного файла."
            };

        public ErrorType(int id) {
            Id = id;
        }

        public int Id { get; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        #region IEquatable<ErrorType>

        public bool Equals(ErrorType other) {
            if(ReferenceEquals(null, other)) {
                return false;
            }

            if(ReferenceEquals(this, other)) {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) {
                return false;
            }

            if(ReferenceEquals(this, obj)) {
                return true;
            }

            if(obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ErrorType) obj);
        }

        public override int GetHashCode() {
            return Id;
        }

        public static bool operator ==(ErrorType left, ErrorType right) {
            return Equals(left, right);
        }

        public static bool operator !=(ErrorType left, ErrorType right) {
            return !Equals(left, right);
        }

        #endregion

        #region IComparable<ErrorType>

        public int CompareTo(ErrorType other) {
            if(ReferenceEquals(this, other)) {
                return 0;
            }

            if(ReferenceEquals(null, other)) {
                return 1;
            }

            return Id.CompareTo(other.Id);
        }

        public int CompareTo(object obj) {
            return CompareTo(obj as ErrorType);
        }

        #endregion

        public override string ToString() {
            return Name;
        }
    }

    internal static class LevelInfoExtensions {
        public static bool IsNotStandard(this LevelInfo levelInfo) {
            return levelInfo.Errors.Count > 0;
        }

        public static bool IsNotElevation(this LevelInfo levelInfo) {
            if(levelInfo.Elevation == null) {
                return false;
            }

            double meterElevation = levelInfo.Level.GetMeterElevation();
            return !LevelExtensions.IsAlmostEqual(levelInfo.Elevation.Value, meterElevation, 0.001);
        }

        public static bool IsNotMillimeterElevation(this LevelInfo levelInfo) {
            double millimeterElevation = levelInfo.Level.GetMillimeterElevation();
            return !LevelExtensions.IsAlmostEqual(millimeterElevation % 1, 0.0000001, 0.0000001);
        }

        public static bool IsNotRangeElevation(this LevelInfo levelInfo, IEnumerable<LevelInfo> levelInfos) {
            if(levelInfo.Errors.Count > 0) {
                return false;
            }

            if(levelInfo.SubLevel.HasValue) {
                return levelInfos
                    .Where(item => item.Errors.Count == 0)
                    .Where(item => item.SubLevel.HasValue)
                    .Where(item=> item.BlockType == levelInfo.BlockType)
                    .Where(item=> item.StartBlock == levelInfo.StartBlock)
                    .Any(item => Math.Abs(levelInfo.Level.GetMillimeterElevation() - item.Level.GetMillimeterElevation()) < 1500);
            }

            if(levelInfo.SubLevel == null) {
                bool first = levelInfos
                    .Where(item => item.Errors.Count == 0)
                    .Where(item=> item.SubLevel == null)
                    .Where(item=> item.LevelNum == levelInfo.LevelNum)
                    .Where(item=> item.BlockType == levelInfo.BlockType)
                    .Any(item => item.StartBlock == levelInfo.StartBlock);

                bool second = levelInfos
                    .Where(item => item.Errors.Count == 0)
                    .Where(item=> item.SubLevel == null)
                    .Where(item=> item.Elevation.HasValue)
                    .Where(item=> item.LevelNum == levelInfo.LevelNum)
                    .Where(item=> item.BlockType == levelInfo.BlockType)
                    .Where(item => item.StartBlock == levelInfo.StartBlock)
                    .Any(item => Math.Abs(item.Elevation ?? 0 - levelInfo.Elevation ?? 0) < double.Epsilon);

                return first || second;
            }

            return false;
        }

        public static bool IsNotFoundLevels(this LevelInfo levelInfo, IEnumerable<LevelInfo> linkLevelInfos) {
            return !linkLevelInfos.Any(item=> item.Level.Name.Equals(levelInfo.Level.Name));
        }

        public static bool IsNotFoundLinkLevels(this LevelInfo linkLevelInfo, IEnumerable<LevelInfo> levelInfos) {
             return !levelInfos.Any(item=> item.Level.Name.Equals(linkLevelInfo.Level.Name));
        }
    }
}