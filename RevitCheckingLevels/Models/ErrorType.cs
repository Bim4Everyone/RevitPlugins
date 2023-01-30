using System;

using Autodesk.Revit.DB;

namespace RevitCheckingLevels.Models {
    internal class ErrorType : IEquatable<ErrorType> {
        public static readonly ErrorType NotStandard =
            new ErrorType() {
                Id = 0,
                Name = "Имена уровней не соответствуют стандарту",
                Description = "Имена уровней должны соответствовать данному формату: \"[Префикс][Номер этажа] [пробел] [«этаж»][\"_\"][Название блока][\".\"][Номер уровня][\"_\"][Отметка уровня]\"."
            };

        public static readonly ErrorType NotElevation =
            new ErrorType() {
                Id = 0,
                Name = "Отметки уровня не соответствуют фактическим",
                Description = $"Имена уровней должны оканчиваться значением параметра \"{LabelUtils.GetLabelFor(BuiltInParameter.LEVEL_ELEV)}\" в метрах с разделителем дробной части в виде точки."
            };

        public static readonly ErrorType NotMillimeterElevation =
            new ErrorType() {
                Id = 0,
                Name = "Отметка уровня не округлена",
                Description = $"Значение параметра \"{LabelUtils.GetLabelFor(BuiltInParameter.LEVEL_ELEV)}\" (в миллиметрах) до 7 знака после запятой должно быть равно \"0\"."
            };

        public static readonly ErrorType NotRangeElevation =
            new ErrorType() {
                Id = 0,
                Name = "Уровни замоделированы не по стандарту",
                Description = $"Значение параметра \"{LabelUtils.GetLabelFor(BuiltInParameter.LEVEL_ELEV)}\" должно отступать на 1500мм от предыдущего."
            };


        public int Id { get; private set; }
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

        public override string ToString() {
            return Name;
        }
    }
}