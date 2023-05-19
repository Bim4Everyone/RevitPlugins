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
                    "Имена уровней должны соответствовать данному формату: \"[Название этажа][\'_\'][Название блока][\'.\'][Номер уровня][\'_\'][Отметка уровня]\"."
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
                Name = "Уровни не найдены в координационном файле",
                Description = "Все уровни проекта должны присутствовать в координационном файле."
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
            if(levelInfo.IsNotStandard()) {
                return false;
            }

            if(levelInfo.Elevation == null) {
                return false;
            }

            double meterElevation = levelInfo.Level.GetMeterElevation();
            return !LevelExtensions.IsAlmostEqual(levelInfo.Elevation.Value, meterElevation, 0.001);
        }

        public static bool IsNotMillimeterElevation(this LevelInfo levelInfo) {
            double millimeterElevation = levelInfo.Level.GetMillimeterElevation();
            millimeterElevation = Math.Round(millimeterElevation, 7, MidpointRounding.AwayFromZero);
            return !LevelExtensions.IsAlmostEqual(millimeterElevation % 1, 0.0000001, 0.0000001);
        }

        public static bool IsNotRangeElevation(this LevelInfo levelInfo, IEnumerable<LevelInfo> levelInfos) {
            if(levelInfo.Errors.Count > 0) {
                return false;
            }

            var filtered = levelInfos
                .Where(item => item.Level.Id != levelInfo.Level.Id)
                .Where(item => item.Errors.Count == 0)
                .ToArray();

            if(levelInfo.HasSubLevel()) {
                return filtered
                    .Where(item => item.HasSubLevel())
                    .Where(item => item.IsEqualBlockName(levelInfo))
                    .Any(item =>
                        Math.Abs(levelInfo.Level.GetMillimeterElevation()
                                 - item.Level.GetMillimeterElevation()) < 1500);
            }

            if(!levelInfo.HasSubLevel()) {
                bool first =  filtered
                    .Where(item => !item.HasSubLevel())
                    .Any(item => item.IsEqualLevelName(levelInfo) && item.IsEqualBlockName(levelInfo));
                
                bool second =  filtered
                    .Where(item => !item.HasSubLevel())
                    .Any(item => item.IsEqualBlockName(levelInfo) && item.IsEqualElevation(levelInfo));

                return first || second;
            }

            return false;
        }

        public static bool IsNotFoundLevels(this LevelInfo levelInfo, IEnumerable<LevelInfo> linkLevelInfos) {
            return !linkLevelInfos.Any(item => item.Level.Name.Equals(levelInfo.Level.Name));
        }

        public static string GetNotStandardTooltip(this LevelInfo levelInfo) {
            return string.Join(Environment.NewLine, levelInfo.Errors);
        }

        public static string GetNotElevationTooltip(this LevelInfo levelInfo) {
            return $"Значение отметки: фактическое \"{levelInfo.Level.GetFormattedMeterElevation()}\", " +
                   $"в имени уровня \"{levelInfo.ElevationName}\".";
        }

        public static string GetNotMillimeterElevationTooltip(this LevelInfo levelInfo) {
            return $"Значение отметки: фактическое \"{levelInfo.Level.GetFormattedMillimeterElevation()}\".";
        }
    }
}