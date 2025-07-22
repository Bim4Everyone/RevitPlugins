using System;
using System.Collections.Generic;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.RebarMarksServices;
using RevitPylonDocumentation.ViewModels;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewMarkCreator {
    private readonly int _formNumberForSkeletonPlatesMax = 2999;
    private readonly int _formNumberForSkeletonPlatesMin = 2001;

    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    private readonly int _formNumberForVerticalRebarMax = 1499;
    private readonly int _formNumberForVerticalRebarMin = 1101;

    private readonly int _formNumberForCBarMax = 1202;
    private readonly int _formNumberForCBarMin = 1202;

    private readonly ParamValueService _paramValueService;
    private readonly RebarFinderService _rebarFinder;

    private readonly TransverseRebarViewBarMarksService _transverseRebarViewBarMarksService;
    private readonly TransverseRebarViewPlateMarksService _transverseRebarViewPlateMarksService;
    private readonly TransverseViewBarMarksService _transverseViewBarMarksService;

    internal PylonViewMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                                  PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _paramValueService = new ParamValueService(repository);
        _rebarFinder = ViewModel.RebarFinder;

        _transverseViewBarMarksService = new TransverseViewBarMarksService(ViewOfPylon, Repository);
        _transverseRebarViewBarMarksService = new TransverseRebarViewBarMarksService(ViewOfPylon, Repository);
        _transverseRebarViewPlateMarksService = new TransverseRebarViewPlateMarksService(ViewOfPylon, Repository);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    public void TryCreateGeneralViewMarks() {

    }

    

    public void TryCreateTransverseViewMarks() {
        var view = ViewOfPylon.ViewElement;
        try {
            CreateTransverseViewBarMarks(view);
        } catch(Exception) { }
    }

    private void CreateTransverseViewBarMarks(View view) {
        var simpleRebars = _rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax,
                                                        _formNumberForCBarMin, _formNumberForCBarMax);
        if(simpleRebars is null) { return; }
        _transverseViewBarMarksService.CreateLeftBottomMark(simpleRebars);

        var simpleClamps = _rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, _formNumberForClampsMin, _formNumberForClampsMax);
        if(simpleClamps != null) {
            _transverseViewBarMarksService.CreateLeftTopMark(simpleClamps, simpleRebars);
            _transverseViewBarMarksService.CreateRightTopMark(simpleClamps, simpleRebars);
        }

        var simpleCBars = _rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, _formNumberForCBarMin);
        if(simpleCBars != null) {
            _transverseViewBarMarksService.CreateLeftMark(simpleCBars, simpleRebars);
        }
    }



    public void TryCreateTransverseRebarViewMarks() {
        var view = ViewOfPylon.ViewElement;
        try {
            CreateTransverseRebarViewBarMarks(view);
            CreateTransverseRebarViewPlateMarks(view);
        } catch(Exception) { }
    }


    private void CreateTransverseRebarViewBarMarks(View view) {
        var simpleRebars = _rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, _formNumberForVerticalRebarMin, _formNumberForVerticalRebarMax);

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


    private void CreateTransverseRebarViewPlateMarks(View view) {
        var simplePlates = _rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, _formNumberForSkeletonPlatesMin, _formNumberForSkeletonPlatesMax);
        if(simplePlates.Count == 0) {
            return;
        }
        _transverseRebarViewPlateMarksService.CreateTopMark(simplePlates);

        _transverseRebarViewPlateMarksService.CreateBottomMark(simplePlates);

        _transverseRebarViewPlateMarksService.CreateLeftMark(simplePlates);

        _transverseRebarViewPlateMarksService.CreateRightMark(simplePlates);
    }
}
