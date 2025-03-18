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
        private readonly RebarFinder _rebarFinder;
        private readonly AnnotationService _annotationService;
        private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
        private readonly FamilySymbol _tagSymbol;
        private readonly FamilySymbol _gostTagSymbol;

        internal PylonViewMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                                      PylonView pylonView) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
            ViewOfPylon = pylonView;

            _paramValueService = new ParamValueService(repository);
            _rebarFinder = new RebarFinder(ViewModel, SheetInfo);
            _annotationService = new AnnotationService(ViewOfPylon);
            _viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);

            // Находим типоразмер марки несущей арматуры
            _tagSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
            // Находим типоразмер типовой аннотации для метки ГОСТа сварки
            _gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }
        internal PylonView ViewOfPylon { get; set; }


        public void TryCreateTransverseRebarViewMarks() {
            View view = ViewOfPylon.ViewElement;
            try {
                CreateRebarMarks(view);
                CreatePlateMarks(view);
            } catch(Exception) { }
        }



        private void CreateRebarMarks(View view) {
            var skeletonRebar = _rebarFinder.GetSkeletonRebar(view);
            var simpleRebars = _rebarFinder.GetSimpleRebars(view, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

            // Определяем наличие в каркасе Г-образных стержней
            var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasFirstLRebarParamName) == 1;
            var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasSecondLRebarParamName) == 1;

            if(firstLRebarParamValue || secondLRebarParamValue) {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                Element leftBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

                // Создаем марку арматуры
                _annotationService.CreateRebarTag(pointLeftBottom, _tagSymbol, leftBottomElement);



                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                // Получаем референс-элемент
                Element leftTopElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftTopElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftTop = _viewPointsAnalyzer.GetPointByDirection(leftTopElement, DirectionType.LeftTop, 1, 0.4, false);

                // Создаем марку арматуры
                _annotationService.CreateRebarTag(pointLeftTop, _tagSymbol, leftTopElement);
            } else {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                // Получаем референс-элемент
                Element leftBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

                // Устанавливаем значение комментария у арматуры, к которой привяжем марку
                leftBottomElement.SetParamValue(_commentParamName, $"{simpleRebars.Count} шт.");

                // Получаем точку в которую нужно поставить аннотацию
                XYZ pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

                // Создаем марку арматуры
                _annotationService.CreateRebarTag(pointLeftBottom, _tagSymbol, leftBottomElement);
            }



            // ПРАВЫЙ НИЖНИЙ УГОЛ
            // Получаем референс-элемент
            Element rightBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightBottom, false);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightBottom = _viewPointsAnalyzer.GetPointByDirection(rightBottomElement, DirectionType.RightBottom, 2, 0.4, false);

            // Создаем типовую аннотацию для обозначения ГОСТа
            _annotationService.CreateGostTag(pointRightBottom, _gostTagSymbol, rightBottomElement);
        }


        private void CreatePlateMarks(View view) {
            var simplePlates = _rebarFinder.GetSimpleRebars(view, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);
            if(simplePlates.Count == 0) {
                return;
            }
            // simplePlates
            // Получаем референс-элемент
            Element topPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Top, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointTopPlateLeader = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.Right, 0.2, 0, true);
            XYZ pointTopPlate = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.LeftBottom, 0.5, 0.4, true);

            // Создаем марку арматуры
            var topPlateTag = _annotationService.CreateRebarTag(pointTopPlate, _tagSymbol, topPlate);
            topPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            topPlateTag.SetLeaderEnd(new Reference(topPlate), pointTopPlateLeader);
#endif


            // Получаем референс-элемент
            Element bottomPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Bottom, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointBottomPlateLeader = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.Left, 0.2, 0, true);
            XYZ pointBottomPlate = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.RightTop, 0.45, 0.3, true);

            // Создаем марку арматуры
            var bottomPlateTag = _annotationService.CreateRebarTag(pointBottomPlate, _tagSymbol, bottomPlate);
            bottomPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            bottomPlateTag.SetLeaderEnd(new Reference(bottomPlate), pointBottomPlateLeader);
#endif



            // Получаем референс-элемент
            Element leftPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Left, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointLeftPlateLeader = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.Bottom, 0.4, 0, true);
            XYZ pointLeftPlate = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.LeftBottom, 0.8, 0.3, true);

            // Создаем марку арматуры
            var leftPlateTag = _annotationService.CreateRebarTag(pointLeftPlate, _tagSymbol, leftPlate);
            leftPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            leftPlateTag.SetLeaderEnd(new Reference(leftPlate), pointLeftPlateLeader);
#endif



            // Получаем референс-элемент
            Element rightPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Right, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightPlateLeader = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.Top, 0.4, 0, true);
            XYZ pointRightPlate = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.RightTop, 0.8, 0.6, true);

            // Создаем марку арматуры
            var rightPlateTag = _annotationService.CreateRebarTag(pointRightPlate, _tagSymbol, rightPlate);
            rightPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            rightPlateTag.SetLeaderEnd(new Reference(rightPlate), pointRightPlateLeader);
#endif
        }
    }
}
