using RevitPylonDocumentation.Models.RebarMarksServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
internal class TransverseViewRebarMarksCreator {
    private readonly int _formNumberForSkeletonPlatesMax = 2999;
    private readonly int _formNumberForSkeletonPlatesMin = 2001;

    private readonly int _formNumberForVerticalRebarMax = 1499;
    private readonly int _formNumberForVerticalRebarMin = 1101;

    private readonly TransverseRebarViewBarMarksService _transverseRebarViewBarMarksService;
    private readonly TransverseRebarViewPlateMarksService _transverseRebarViewPlateMarksService;
    
    internal TransverseViewRebarMarksCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _transverseRebarViewBarMarksService = new TransverseRebarViewBarMarksService(ViewOfPylon, Repository);
        _transverseRebarViewPlateMarksService = new TransverseRebarViewPlateMarksService(ViewOfPylon, Repository);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }

    internal void CreateTransverseRebarViewBarMarks() {
        var simpleRebars = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

        // Если у нас есть Г-образные стержни или стержни разной длины, то нужно ставить две разные марки
        // Если нет - то допускается поставить одну марку, которая будет характеризовать все стрежни (они же одинаковые)
        if(SheetInfo.RebarInfo.FirstLRebarParamValue
            || SheetInfo.RebarInfo.SecondLRebarParamValue
            || SheetInfo.RebarInfo.DifferentRebarParamValue) {
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


    internal void CreateTransverseRebarViewPlateMarks() {
        var simplePlates = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);
        if(simplePlates.Count == 0) {
            return;
        }
        _transverseRebarViewPlateMarksService.CreateTopMark(simplePlates);

        _transverseRebarViewPlateMarksService.CreateBottomMark(simplePlates);

        _transverseRebarViewPlateMarksService.CreateLeftMark(simplePlates);

        _transverseRebarViewPlateMarksService.CreateRightMark(simplePlates);
    }
}
