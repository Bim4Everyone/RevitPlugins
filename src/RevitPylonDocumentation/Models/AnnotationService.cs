using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models {
    internal class AnnotationService {
        private readonly PylonView _pylonView;

        private readonly string _annotationTagTopTextParamName = "Текст верх";
        private readonly string _annotationTagLengthParamName = "Ширина полки";
        private readonly string _weldingGostText = "ГОСТ 14098-2014-Н1-Рш";


        public AnnotationService(PylonView pylonView) {
            _pylonView = pylonView;
        }


        public IndependentTag CreateRebarTag(XYZ bodyPoint, FamilySymbol tagSymbol, Element element) {
            var view = _pylonView.ViewElement;
            var doc = view.Document;
            var annotationInstance = IndependentTag.Create(doc, tagSymbol.Id, view.Id, new Reference(element),
                              true, TagOrientation.Horizontal, bodyPoint);
            annotationInstance.TagHeadPosition = bodyPoint;
            return annotationInstance;
        }


        public void CreateGostTag(XYZ bodyPoint, FamilySymbol annotationSymbol, Element element) {
            var view = _pylonView.ViewElement;
            var doc = view.Document;
            // Создаем экземпляр типовой аннотации для указания ГОСТа
            AnnotationSymbol annotationInstance = doc.Create.NewFamilyInstance(
                bodyPoint,
                annotationSymbol,
                view) as AnnotationSymbol;

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
