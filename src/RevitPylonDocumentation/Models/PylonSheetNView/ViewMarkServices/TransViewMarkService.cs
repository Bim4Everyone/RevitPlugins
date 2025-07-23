using System;

using RevitPylonDocumentation.Models.RebarMarksServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class TransViewMarkService {
    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    private readonly int _formNumberForVerticalRebarMax = 1499;
    private readonly int _formNumberForVerticalRebarMin = 1101;

    private readonly int _formNumberForCBarMax = 1202;
    private readonly int _formNumberForCBarMin = 1202;

    private readonly BarMarksService _transverseViewBarMarksService;

    internal TransViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                       PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _transverseViewBarMarksService = new BarMarksService(ViewOfPylon, Repository);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }

    internal void TryCreateTransverseViewBarMarks() {
        try {
            var simpleRebars = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, 
                                                                     _formNumberForVerticalRebarMin, 
                                                                     _formNumberForVerticalRebarMax,
                                                                     _formNumberForCBarMin, 
                                                                     _formNumberForCBarMax);
            if(simpleRebars is null) { return; }
            _transverseViewBarMarksService.CreateLeftBottomMark(simpleRebars);

            var simpleClamps = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, 
                                                                     _formNumberForClampsMin, _formNumberForClampsMax);
            if(simpleClamps != null) {
                _transverseViewBarMarksService.CreateLeftTopMark(simpleClamps, simpleRebars);
                _transverseViewBarMarksService.CreateRightTopMark(simpleClamps, simpleRebars);
            }

            var simpleCBars = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, 
                                                                    _formNumberForCBarMin);
            if(simpleCBars != null) {
                _transverseViewBarMarksService.CreateLeftMark(simpleCBars, simpleRebars);
            }
        } catch(Exception) { }
    }
}


