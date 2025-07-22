using System;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
internal class TransViewThirdMarkCreator : ViewMarkCreator {
    public TransViewThirdMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewMarks() {
        try {
            var creator = new TransverseViewMarksCreator(ViewModel, Repository, SheetInfo, ViewOfPylon);
            creator.CreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
