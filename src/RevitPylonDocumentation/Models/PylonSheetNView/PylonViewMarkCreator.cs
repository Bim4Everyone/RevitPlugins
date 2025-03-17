using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewMarkCreator {
        private readonly string _commentParamName = "Комментарии";

        private readonly int _formNumberForVerticalRebarMax = 1499;
        private readonly int _formNumberForVerticalRebarMin = 1101;
        private readonly int _formNumberForSkeletonPlatesMax = 2999;
        private readonly int _formNumberForSkeletonPlatesMin = 2001;

        private readonly string _hasFirstLRebarParamName = "ст_Г_1_ВКЛ";
        private readonly string _hasSecondLRebarParamName = "ст_Г_2_ВКЛ";

        private readonly ParamValueService _paramValueService;
        private readonly FamilySymbol _tagSymbol;

        internal PylonViewMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;

            _paramValueService = new ParamValueService(repository);

            // Находим типоразмер марки несущей арматуры
            _tagSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        public void TryCreateTransverseRebarViewFirstMarks() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;

            try {
                var annotationService = new AnnotationService(view);
                // Получаем арматуру (не родительское семейство, а элементы, которые уходят в спецификацию)
                var rebarFinder = new RebarFinder(ViewModel, SheetInfo);

                // Определяем координаты углов вида
                ViewPointsAnalyzer viewPointsAnalyzer = new ViewPointsAnalyzer(view);
                viewPointsAnalyzer.AnalyzeCorners();

                CreateRebarMarks(view, rebarFinder, viewPointsAnalyzer, annotationService);
                CreatePlateMarks(view, rebarFinder, viewPointsAnalyzer, annotationService);
            } catch(Exception) { }
        }


        public void TryCreateTransverseRebarViewSecondMarks() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewSecond.ViewElement;

            try {
                var annotationService = new AnnotationService(view);
                // Получаем арматуру (не родительское семейство, а элементы, которые уходят в спецификацию)
                var rebarFinder = new RebarFinder(ViewModel, SheetInfo);

                // Определяем координаты углов вида
                ViewPointsAnalyzer viewPointsAnalyzer = new ViewPointsAnalyzer(view);
                viewPointsAnalyzer.AnalyzeCorners();

                CreateRebarMarks(view, rebarFinder, viewPointsAnalyzer, annotationService);
                CreatePlateMarks(view, rebarFinder, viewPointsAnalyzer, annotationService);
            } catch(Exception) { }
        }




        private void CreateRebarMarks(View view, RebarFinder rebarFinder, ViewPointsAnalyzer viewPointsAnalyzer,
                                      AnnotationService annotationService) {
            var skeletonRebar = rebarFinder.GetSkeletonRebar(view);
            var simpleRebars = rebarFinder.GetSimpleRebars(view, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

            // Находим типоразмер типовой аннотации для метки ГОСТа сварки
            FamilySymbol annotationSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");


            // Определяем наличие в каркасе Г-образных стержней
            var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasFirstLRebarParamName) == 1;
            var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasSecondLRebarParamName) == 1;

            if(firstLRebarParamValue || secondLRebarParamValue) {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                Element leftBottomElement = viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = viewPointsAnalyzer.GetPointByDirection(view, leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

                // Создаем марку арматуры
                annotationService.CreateRebarTag(pointLeftBottom, _tagSymbol, leftBottomElement);



                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                // Получаем референс-элемент
                Element leftTopElement = viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftTopElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftTop = viewPointsAnalyzer.GetPointByDirection(view, leftTopElement, DirectionType.LeftTop, 1, 0.4, false);

                // Создаем марку арматуры
                annotationService.CreateRebarTag(pointLeftTop, _tagSymbol, leftTopElement);
            } else {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                Element leftBottomElement = viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = viewPointsAnalyzer.GetPointByDirection(view, leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

                // Создаем марку арматуры
                annotationService.CreateRebarTag(pointLeftBottom, _tagSymbol, leftBottomElement);
            }



            // ПРАВЫЙ НИЖНИЙ УГОЛ
            // Получаем референс-элемент
            Element rightBottomElement = viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightBottom, false);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightBottom = viewPointsAnalyzer.GetPointByDirection(view, rightBottomElement, DirectionType.RightBottom, 2, 0.4, false);

            // Создаем типовую аннотацию для обозначения ГОСТа
            annotationService.CreateGostTag(pointRightBottom, annotationSymbol, rightBottomElement);
        }


        private void CreatePlateMarks(View view, RebarFinder rebarFinder, ViewPointsAnalyzer viewPointsAnalyzer,
                                      AnnotationService annotationService) {
            var simplePlates = rebarFinder.GetSimpleRebars(view, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);
            if(simplePlates.Count == 0) {
                return;
            }
            // simplePlates
            // Получаем референс-элемент
            Element topPlate = viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Top, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointTopPlateLeader = viewPointsAnalyzer.GetPointByDirection(view, topPlate, DirectionType.Right, 0.2, 0, true);
            XYZ pointTopPlate = viewPointsAnalyzer.GetPointByDirection(view, topPlate, DirectionType.LeftBottom, 0.5, 0.4, true);

            // Создаем марку арматуры
            var topPlateTag = annotationService.CreateRebarTag(pointTopPlate, _tagSymbol, topPlate);
            topPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            topPlateTag.SetLeaderEnd(new Reference(topPlate), pointTopPlateLeader);
#endif


            // Получаем референс-элемент
            Element bottomPlate = viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Bottom, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointBottomPlateLeader = viewPointsAnalyzer.GetPointByDirection(view, bottomPlate, DirectionType.Left, 0.2, 0, true);
            XYZ pointBottomPlate = viewPointsAnalyzer.GetPointByDirection(view, bottomPlate, DirectionType.RightTop, 0.45, 0.3, true);

            // Создаем марку арматуры
            var bottomPlateTag = annotationService.CreateRebarTag(pointBottomPlate, _tagSymbol, bottomPlate);
            bottomPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            bottomPlateTag.SetLeaderEnd(new Reference(bottomPlate), pointBottomPlateLeader);
#endif



            // Получаем референс-элемент
            Element leftPlate = viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Left, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointLeftPlateLeader = viewPointsAnalyzer.GetPointByDirection(view, leftPlate, DirectionType.Bottom, 0.4, 0, true);
            XYZ pointLeftPlate = viewPointsAnalyzer.GetPointByDirection(view, leftPlate, DirectionType.LeftBottom, 0.8, 0.3, true);

            // Создаем марку арматуры
            var leftPlateTag = annotationService.CreateRebarTag(pointLeftPlate, _tagSymbol, leftPlate);
            leftPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            leftPlateTag.SetLeaderEnd(new Reference(leftPlate), pointLeftPlateLeader);
#endif



            // Получаем референс-элемент
            Element rightPlate = viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Right, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightPlateLeader = viewPointsAnalyzer.GetPointByDirection(view, rightPlate, DirectionType.Top, 0.4, 0, true);
            XYZ pointRightPlate = viewPointsAnalyzer.GetPointByDirection(view, rightPlate, DirectionType.RightTop, 0.8, 0.6, true);

            // Создаем марку арматуры
            var rightPlateTag = annotationService.CreateRebarTag(pointRightPlate, _tagSymbol, rightPlate);
            rightPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            rightPlateTag.SetLeaderEnd(new Reference(rightPlate), pointRightPlateLeader);
#endif
        }
    }
}
