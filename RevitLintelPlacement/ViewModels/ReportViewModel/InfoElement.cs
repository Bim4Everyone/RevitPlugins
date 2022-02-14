using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.ViewModels {
    internal class InfoElement {

        public static InfoElement LintelIsFixedWithoutElement => new InfoElement() { 
            Message = "Под зафиксиованной перемычкой отсутствует проем.",
            TypeInfo = TypeInfo.Warning,
            ElementType = ElementType.Lintel
        };

        public static InfoElement LintelGeometricalDisplaced => new InfoElement() {
            Message = "Перемычка смещена от проема.",
            TypeInfo = TypeInfo.Warning,
            ElementType = ElementType.Lintel
        };

        public string Message { get; set; }
        public TypeInfo TypeInfo { get; set; }
        public ElementType ElementType { get; set; }
        public InfoElement FormatMessage(params string[] args) {
            return new InfoElement() {
                TypeInfo = TypeInfo,
                Message = string.Format(Message, args)
            };
        }
    }
}
