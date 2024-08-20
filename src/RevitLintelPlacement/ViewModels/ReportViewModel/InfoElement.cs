using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.ViewModels {
    internal class InfoElement {

        public static InfoElement LintelIsFixedWithoutElement => new InfoElement() { 
            Message = "Под зафиксированной перемычкой отсутствует проем.",
            TypeInfo = TypeInfo.Warning,
            ElementType = ElementType.Lintel
        };

        public static InfoElement LintelGeometricalDisplaced => new InfoElement() {
            Message = "Перемычка смещена от проема.",
            TypeInfo = TypeInfo.Warning,
            ElementType = ElementType.Lintel
        };

        public static InfoElement MissingLintelParameter => new InfoElement() {
            Message = "У семейства перемычки отсутствует значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Lintel
        };

        public static InfoElement MissingOpeningParameter => new InfoElement() {
            Message = "У проема отсутствует значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Opening
        };

        public static InfoElement UnsetLintelParamter => new InfoElement() {
            Message = "У перемычки не удалось установить значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Opening
        };

        public static InfoElement BlankParamter => new InfoElement() {
            Message = "В настройках не установлено значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Config
        };

        public static InfoElement LackOfView => new InfoElement() {
            Message = "В проекте не содержится ни одного вида \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.View
        };

        public string Message { get; set; }
        public TypeInfo TypeInfo { get; set; }
        public ElementType ElementType { get; set; }
        public InfoElement FormatMessage(params string[] args) {
            return new InfoElement() {
                TypeInfo = TypeInfo,
                Message = string.Format(Message, args),
                ElementType = ElementType
            };
        }
    }
}
