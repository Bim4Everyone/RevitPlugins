using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonRebarInfo {
    private readonly string _hasFirstLRebarParamName = "ст_Г_1_ВКЛ";
    private readonly string _hasSecondLRebarParamName = "ст_Г_2_ВКЛ";
    private readonly string _hasDifferentRebarParamName = "ст_РАЗНЫЕ";

    internal PylonRebarInfo(MainViewModel mvm, RevitRepository revitRepository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        SheetInfo = pylonSheetInfo;
        Repository = revitRepository;

        SkeletonParentRebar = ViewModel.RebarFinder.GetSkeletonParentRebar(SheetInfo.ProjectSection, SheetInfo.PylonKeyName);
        GetInfo();
    }

    internal MainViewModel ViewModel { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal RevitRepository Repository { get; set; }

    internal FamilyInstance SkeletonParentRebar { get; set; }
    internal bool FirstLRebarParamValue { get; set; } = false;
    internal bool SecondLRebarParamValue { get; set; } = false;
    internal bool DifferentRebarParamValue { get; set; } = false;
    internal bool AllRebarAreL { get; set; } = false;
    internal bool HasLRebar { get; set; } = false;


    private void GetInfo() {
        if(SkeletonParentRebar is null) { return; }
        
        // Определяем наличие в каркасе Г-образных стержней
        FirstLRebarParamValue = ViewModel.ParamValService.GetParamValueAnywhere(SkeletonParentRebar, _hasFirstLRebarParamName) == 1;
        SecondLRebarParamValue = ViewModel.ParamValService.GetParamValueAnywhere(SkeletonParentRebar, _hasSecondLRebarParamName) == 1;
        DifferentRebarParamValue = ViewModel.ParamValService.GetParamValueAnywhere(SkeletonParentRebar, _hasDifferentRebarParamName) == 1;

        AllRebarAreL = FirstLRebarParamValue && SecondLRebarParamValue;
        HasLRebar = FirstLRebarParamValue || SecondLRebarParamValue;
    }
}
