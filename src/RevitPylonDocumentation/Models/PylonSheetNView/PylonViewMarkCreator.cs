using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewMarkCreator {
        private readonly string _commentParamName = "Комментарии";
        private readonly string _annotationTagTopTextParamName = "Текст верх";
        private readonly string _annotationTagLengthParamName = "Ширина полки";
        private readonly string _weldingGostText = "ГОСТ 14098-2014-Н1-Рш";

        private readonly int _formNumberForVerticalRebarMax = 1499;
        private readonly int _formNumberForVerticalRebarMin = 1101;
        private readonly int _formNumberForSkeletonPlatesMax = 2999;
        private readonly int _formNumberForSkeletonPlatesMin = 2001;

        private readonly string _hasFirstLRebarParamName = "ст_Г_1_ВКЛ";
        private readonly string _hasSecondLRebarParamName = "ст_Г_2_ВКЛ";

        private readonly ParamValueService _paramValueService;

        internal PylonViewMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;

            _paramValueService = new ParamValueService(repository);
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        public void TryCreateTransverseRebarViewFirstMarks() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;

            // Получаем арматуру (не родительское семейство, а элементы, которые уходят в спецификацию)
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);

            var skeletonRebar = rebarFinder.GetSkeletonRebar(view);

            var simpleRebars = rebarFinder.GetSimpleRebars(view, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

            var simplePlates = rebarFinder.GetSimpleRebars(view, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);


            // Определяем координаты углов вида
            ViewCornersAnalyzer cornersAnalyzer = new ViewCornersAnalyzer(view);
            cornersAnalyzer.AnalyzeCorners();

            // Находим типоразмер марки несущей арматуры
            FamilySymbol tagSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
            // Находим типоразмер типовой аннотации для метки ГОСТа сварки
            FamilySymbol annotationSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");




            // Определяем наличие в каркасе Г-образных стержней
            var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasFirstLRebarParamName) == 1;
            var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasSecondLRebarParamName) == 1;

            if(firstLRebarParamValue || secondLRebarParamValue) {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                FamilyInstance leftBottomElement = cornersAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false) as FamilyInstance;

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = cornersAnalyzer.GetPointByDirection(view, leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

                // Создаем марку арматуры
                CreateRebarTag(view, pointLeftBottom, tagSymbol, leftBottomElement);



                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                // Получаем референс-элемент
                FamilyInstance leftTopElement = cornersAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false) as FamilyInstance;

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftTopElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftTop = cornersAnalyzer.GetPointByDirection(view, leftTopElement, DirectionType.LeftTop, 1, 0.4, false);

                // Создаем марку арматуры
                CreateRebarTag(view, pointLeftTop, tagSymbol, leftTopElement);
            } else {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                FamilyInstance leftBottomElement = cornersAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false) as FamilyInstance;

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = cornersAnalyzer.GetPointByDirection(view, leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

                // Создаем марку арматуры
                CreateRebarTag(view, pointLeftBottom, tagSymbol, leftBottomElement);
            }






            // ПРАВЫЙ НИЖНИЙ УГОЛ
            // Получаем референс-элемент
            FamilyInstance rightBottomElement = cornersAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightBottom, false) as FamilyInstance;

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightBottom = cornersAnalyzer.GetPointByDirection(view, rightBottomElement, DirectionType.RightBottom, 2, 0.4, false);

            // Создаем типовую аннотацию для обозначения ГОСТа
            CreateGostTag(view, pointRightBottom, annotationSymbol, rightBottomElement);



            if(simplePlates.Count == 0) {
                return;
            }
            // simplePlates
            // Получаем референс-элемент
            Element topPlate = cornersAnalyzer.GetElementByDirection(simplePlates, DirectionType.Top, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointTopPlateLeader = cornersAnalyzer.GetPointByDirection(view, topPlate, DirectionType.Right, 0.2, 0, true);
            XYZ pointTopPlate = cornersAnalyzer.GetPointByDirection(view, topPlate, DirectionType.LeftBottom, 0.5, 0.4, true);

            // Создаем марку арматуры
            var topPlateTag = CreateRebarTag(view, pointTopPlate, tagSymbol, topPlate);
            topPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            topPlateTag.SetLeaderEnd(new Reference(topPlate), pointTopPlateLeader);
#endif


            // Получаем референс-элемент
            Element bottomPlate = cornersAnalyzer.GetElementByDirection(simplePlates, DirectionType.Bottom, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointBottomPlateLeader = cornersAnalyzer.GetPointByDirection(view, bottomPlate, DirectionType.Left, 0.2, 0, true);
            XYZ pointBottomPlate = cornersAnalyzer.GetPointByDirection(view, bottomPlate, DirectionType.RightTop, 0.45, 0.3, true);

            // Создаем марку арматуры
            var bottomPlateTag = CreateRebarTag(view, pointBottomPlate, tagSymbol, bottomPlate);
            bottomPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            bottomPlateTag.SetLeaderEnd(new Reference(bottomPlate), pointBottomPlateLeader);
#endif











            // Получаем референс-элемент
            Element leftPlate = cornersAnalyzer.GetElementByDirection(simplePlates, DirectionType.Left, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointLeftPlateLeader = cornersAnalyzer.GetPointByDirection(view, leftPlate, DirectionType.Bottom, 0.4, 0, true);
            XYZ pointLeftPlate = cornersAnalyzer.GetPointByDirection(view, leftPlate, DirectionType.LeftBottom, 0.8, 0.3, true);

            // Создаем марку арматуры
            var leftPlateTag = CreateRebarTag(view, pointLeftPlate, tagSymbol, leftPlate);
            leftPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            leftPlateTag.SetLeaderEnd(new Reference(leftPlate), pointLeftPlateLeader);
#endif




            // Получаем референс-элемент
            Element rightPlate = cornersAnalyzer.GetElementByDirection(simplePlates, DirectionType.Right, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightPlateLeader = cornersAnalyzer.GetPointByDirection(view, rightPlate, DirectionType.Top, 0.4, 0, true);
            XYZ pointRightPlate = cornersAnalyzer.GetPointByDirection(view, rightPlate, DirectionType.RightTop, 0.8, 0.6, true);

            // Создаем марку арматуры
            var rightPlateTag = CreateRebarTag(view, pointRightPlate, tagSymbol, rightPlate);
            rightPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            rightPlateTag.SetLeaderEnd(new Reference(rightPlate), pointRightPlateLeader);
#endif


            try {

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
