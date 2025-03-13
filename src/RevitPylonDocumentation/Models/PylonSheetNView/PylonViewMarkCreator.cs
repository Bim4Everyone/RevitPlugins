using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewMarkCreator {
        private readonly string _commentParamName = "Комментарии";
        private readonly string _annotationTagTopTextParamName = "Текст верх";
        private readonly string _annotationTagLengthParamName = "Ширина полки";
        private readonly string _weldingGostText = "ГОСТ 14098-2014-Н1-Рш";

        internal PylonViewMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        public void TryCreateTransverseRebarViewFirstMarks() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;

            try {
                // Получаем арматуру (не родительское семейство, а элементы, которые уходят в спецификацию)
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var simpleRebars = rebarFinder.GetSimpleRebars(view);

                // Определяем координаты углов вида
                ViewCornersAnalyzer cornersAnalyzer = new ViewCornersAnalyzer(view);
                cornersAnalyzer.AnalyzeCorners();

                // Находим типоразмер марки несущей арматуры
                FamilySymbol tagSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
                // Находим типоразмер типовой аннотации для метки ГОСТа сварки
                FamilySymbol annotationSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");


                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                FamilyInstance leftBottomElement = cornersAnalyzer.GetElementByCorner(simpleRebars, CornerType.LeftBottom) as FamilyInstance;

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = cornersAnalyzer.GetPointByCorner(view, leftBottomElement, CornerType.LeftBottom);

                // Создаем марку арматуры
                CreateRebarTag(view, pointLeftBottom, tagSymbol, leftBottomElement);



                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                // Получаем референс-элемент
                FamilyInstance leftTopElement = cornersAnalyzer.GetElementByCorner(simpleRebars, CornerType.LeftTop) as FamilyInstance;

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftTopElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftTop = cornersAnalyzer.GetPointByCorner(view, leftTopElement, CornerType.LeftTop);

                // Создаем марку арматуры
                CreateRebarTag(view, pointLeftTop, tagSymbol, leftTopElement);



                // ПРАВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                FamilyInstance rightBottomElement = cornersAnalyzer.GetElementByCorner(simpleRebars, CornerType.RightBottom) as FamilyInstance;

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointRightBottom = cornersAnalyzer.GetPointByCorner(view, rightBottomElement, CornerType.RightBottom);

                // Создаем типовую аннотацию для обозначения ГОСТа
                CreateGostTag(view, pointRightBottom, annotationSymbol, rightBottomElement);
            } catch(Exception) { }
        }



        private IndependentTag CreateRebarTag(View view, XYZ bodyPoint, FamilySymbol tagSymbol, Element element) {
            var doc = Repository.Document;
            var annotationInstance = IndependentTag.Create(doc, tagSymbol.Id, view.Id, new Reference(element),
                              true, TagOrientation.Horizontal, bodyPoint);
            annotationInstance.TagHeadPosition = bodyPoint;
            return annotationInstance;
        }


        private void CreateGostTag(View view, XYZ bodyPoint, FamilySymbol annotationSymbol, Element element) {
            var doc = Repository.Document;
            // Создаем экземпляр типовой аннотации для указания ГОСТа
            AnnotationSymbol annotationInstance = doc.Create.NewFamilyInstance(
                bodyPoint, // Точка размещения тела аннотации
                annotationSymbol,    // Типоразмер аннотации
                view) as AnnotationSymbol;               // Вид, в котором размещается аннотация

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
