using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonDocumentation.Models {
    internal class AnnotationService {
        private readonly View _view;

        private readonly string _annotationTagTopTextParamName = "Текст верх";
        private readonly string _annotationTagLengthParamName = "Ширина полки";
        private readonly string _weldingGostText = "ГОСТ 14098-2014-Н1-Рш";


        public AnnotationService(View view) {
            _view = view;
        }


        public IndependentTag CreateRebarTag(XYZ bodyPoint, FamilySymbol tagSymbol, Element element) {
            var doc = _view.Document;
            var annotationInstance = IndependentTag.Create(doc, tagSymbol.Id, _view.Id, new Reference(element),
                              true, TagOrientation.Horizontal, bodyPoint);
            annotationInstance.TagHeadPosition = bodyPoint;
            return annotationInstance;
        }


        public void CreateGostTag(XYZ bodyPoint, FamilySymbol annotationSymbol, Element element) {
            var doc = _view.Document;
            // Создаем экземпляр типовой аннотации для указания ГОСТа
            AnnotationSymbol annotationInstance = doc.Create.NewFamilyInstance(
                bodyPoint,
                annotationSymbol,
                _view) as AnnotationSymbol;

            // Устанавливаем значение верхнего текста у выноски
            annotationInstance.SetParamValue(_annotationTagTopTextParamName, _weldingGostText);

            // Устанавливаем значение длины полки под текстом, чтобы текст влез
            annotationInstance.SetParamValue(_annotationTagLengthParamName, UnitUtilsHelper.ConvertToInternalValue(40));

            // Добавляем и устанавливаем точку привязки выноски
            annotationInstance.addLeader();
            Leader leader = annotationInstance.GetLeaders().FirstOrDefault();
            if(leader != null) {
                var loc = element.Location as LocationPoint;
                leader.End = loc.Point; // Точка на элементе
            }
        }
    }
}
