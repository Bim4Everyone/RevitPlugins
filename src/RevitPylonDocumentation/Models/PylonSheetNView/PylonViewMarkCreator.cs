using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewMarkCreator {
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

        private readonly TransverseRebarBarMarksService _transverseRebarBarMarksService;
        private readonly TransverseRebarPlateMarksService _transverseRebarPlateMarksService;
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

            _transverseRebarBarMarksService = new TransverseRebarBarMarksService(ViewOfPylon, Repository);
            _transverseRebarPlateMarksService = new TransverseRebarPlateMarksService(ViewOfPylon, Repository);

            // Находим типоразмер марки несущей арматуры
            _tagSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
            // Находим типоразмер типовой аннотации для метки ГОСТа сварки
            _gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }
        internal PylonView ViewOfPylon { get; set; }


        public void TryCreateTransverseRebarMarks() {
            View view = ViewOfPylon.ViewElement;
            try {
                CreateTransverseBarMarks(view);
                CreatePlateMarks(view);
            } catch(Exception) { }
        }



        private void CreateTransverseBarMarks(View view) {
            var skeletonRebar = _rebarFinder.GetSkeletonRebar(view);
            var simpleRebars = _rebarFinder.GetSimpleRebars(view, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

            // Определяем наличие в каркасе Г-образных стержней
            var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasFirstLRebarParamName) == 1;
            var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonRebar, _hasSecondLRebarParamName) == 1;

            if(firstLRebarParamValue || secondLRebarParamValue) {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                _transverseRebarBarMarksService.CreateLeftBottomMarks(simpleRebars, true);

                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                _transverseRebarBarMarksService.CreateLeftTopMarks(simpleRebars);
            } else {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                _transverseRebarBarMarksService.CreateLeftBottomMarks(simpleRebars, false);
            }
            // ПРАВЫЙ НИЖНИЙ УГОЛ
            _transverseRebarBarMarksService.CreateRightBottomMarks(simpleRebars);
        }


        private void CreatePlateMarks(View view) {
            var simplePlates = _rebarFinder.GetSimpleRebars(view, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);
            if(simplePlates.Count == 0) {
                return;
            }
            _transverseRebarPlateMarksService.CreateTransversePlateTopMarks(simplePlates);

            _transverseRebarPlateMarksService.CreateTransversePlateBottomMarks(simplePlates);

            _transverseRebarPlateMarksService.CreateTransversePlateLeftMarks(simplePlates);

            _transverseRebarPlateMarksService.CreateTransversePlateRightMarks(simplePlates);
        }
    }
}
