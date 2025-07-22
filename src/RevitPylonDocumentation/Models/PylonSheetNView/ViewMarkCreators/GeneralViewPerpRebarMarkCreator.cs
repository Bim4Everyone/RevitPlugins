using System;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
internal class GeneralViewPerpRebarMarkCreator : ViewMarkCreator {
    public GeneralViewPerpRebarMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewMarks() {
        throw new NotImplementedException();
    }
}
