using System;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
internal class TransViewSecondRebarMarkCreator : ViewMarkCreator {
    public TransViewSecondRebarMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewMarks() {
        try {
            var creator = new TransverseViewRebarMarksCreator(ViewModel, Repository, SheetInfo, ViewOfPylon);
            creator.CreateTransverseRebarViewBarMarks();
            creator.CreateTransverseRebarViewPlateMarks();
        } catch(Exception) { }
    }
}
