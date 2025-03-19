using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.RebarMarksServices;
using RevitPylonDocumentation.ViewModels;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewMarkCreator {
        private readonly int _formNumberForVerticalRebarMax = 1499;
        private readonly int _formNumberForVerticalRebarMin = 1101;
        private readonly int _formNumberForSkeletonPlatesMax = 2999;
        private readonly int _formNumberForSkeletonPlatesMin = 2001;

        private readonly int _formNumberForClampsMax = 1599;
        private readonly int _formNumberForClampsMin = 1500;

        private readonly string _hasFirstLRebarParamName = "ст_Г_1_ВКЛ";
        private readonly string _hasSecondLRebarParamName = "ст_Г_2_ВКЛ";

        private readonly ParamValueService _paramValueService;
        private readonly RebarFinder _rebarFinder;
        private readonly AnnotationService _annotationService;
        private readonly ViewPointsAnalyzer _viewPointsAnalyzer;

        private readonly TransverseRebarViewBarMarksService _transverseRebarViewBarMarksService;
        private readonly TransverseRebarViewPlateMarksService _transverseRebarViewPlateMarksService;
        private readonly TransverseViewBarMarksService _transverseViewBarMarksService;
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

            _transverseViewBarMarksService = new TransverseViewBarMarksService(ViewOfPylon, Repository);

            _transverseRebarViewBarMarksService = new TransverseRebarViewBarMarksService(ViewOfPylon, Repository);
            _transverseRebarViewPlateMarksService = new TransverseRebarViewPlateMarksService(ViewOfPylon, Repository);

            // Находим типоразмер марки несущей арматуры
            _tagSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
            // Находим типоразмер типовой аннотации для метки ГОСТа сварки
            _gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }
        internal PylonView ViewOfPylon { get; set; }


        public void TryCreateTransverseViewMarks() {
            View view = ViewOfPylon.ViewElement;
            try {
                CreateTransverseViewBarMarks(view);
            } catch(Exception) { }
        }

        private void CreateTransverseViewBarMarks(View view) {
            var simpleClamps = _rebarFinder.GetSimpleRebars(view, _formNumberForClampsMin, _formNumberForClampsMax);
            var simpleRebars = _rebarFinder.GetSimpleRebars(view, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

            _transverseViewBarMarksService.CreateLeftTopMarks(simpleClamps, simpleRebars);

        }





        public void TryCreateTransverseRebarViewMarks() {
            View view = ViewOfPylon.ViewElement;
            try {
                CreateTransverseRebarViewBarMarks(view);
                CreatePlateMarks(view);
            } catch(Exception) { }
        }



        private void CreateTransverseRebarViewBarMarks(View view) {
            var skeletonRebar = _rebarFinder.GetSkeletonRebar(view);
            var simpleRebars = _rebarFinder.GetSimpleRebars(view, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

            // Определяем наличие в каркасе Г-образных стержней
            var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasFirstLRebarParamName) == 1;
            var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasSecondLRebarParamName) == 1;

            if(firstLRebarParamValue || secondLRebarParamValue) {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                _transverseRebarViewBarMarksService.CreateLeftBottomMark(simpleRebars, true);

                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                _transverseRebarViewBarMarksService.CreateLeftTopMark(simpleRebars);
            } else {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                _transverseRebarViewBarMarksService.CreateLeftBottomMark(simpleRebars, false);
            }
            // ПРАВЫЙ НИЖНИЙ УГОЛ
            _transverseRebarViewBarMarksService.CreateRightBottomMark(simpleRebars);
        }


        private void CreatePlateMarks(View view) {
            var simplePlates = _rebarFinder.GetSimpleRebars(view, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);
            if(simplePlates.Count == 0) {
                return;
            }
            _transverseRebarViewPlateMarksService.CreateTransversePlateTopMark(simplePlates);

            _transverseRebarViewPlateMarksService.CreateTransversePlateBottomMark(simplePlates);

            _transverseRebarViewPlateMarksService.CreateTransversePlateLeftMark(simplePlates);

            _transverseRebarViewPlateMarksService.CreateTransversePlateRightMark(simplePlates);
        }
    }
}
