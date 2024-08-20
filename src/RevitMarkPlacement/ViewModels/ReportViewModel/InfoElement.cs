using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMarkPlacement.ViewModels {
    internal class InfoElement {

        public static InfoElement FamilyAnnotationMissing => new InfoElement() {
            Message = "Отсутствует семейство типовой аннотации \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Annotation
        };

        public static InfoElement AnnotationParameterMissing => new InfoElement() {
            Message = "У семейства \"{0}\" отсутствует значение параметра \"{1}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Annotation
        };

        public static InfoElement ElevationParameterMissing => new InfoElement() {
            Message = "У высотной отметки \"{0}\" отсутствует значение параметра \"{1}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.SpotDimension
        };

        public string Message { get; set; }
        public TypeInfo TypeInfo { get; set; }
        public ElementType ElementType { get; set; }

        public InfoElement FormatMessage(params string[] args)  {
            return new InfoElement() {
                TypeInfo = TypeInfo,
                Message = string.Format(Message, args),
                ElementType = ElementType
            };
        }
    }
}
