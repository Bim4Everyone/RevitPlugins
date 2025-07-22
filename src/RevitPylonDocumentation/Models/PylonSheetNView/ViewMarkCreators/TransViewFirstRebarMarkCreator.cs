using System;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
internal class TransViewFirstRebarMarkCreator : ViewMarkCreator {
    public TransViewFirstRebarMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
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
