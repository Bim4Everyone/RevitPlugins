namespace RevitDeclarations.Models;
internal class ContourCheckResult {
    public ContourCheckResult() {
        MainResult = false;
        NeedToCheck = false;
        HasError = false;
    }

    public bool MainResult { get; set; }
    public bool HasError { get; set; }
    public bool NeedToCheck { get; set; }

    public ContourCheckEnum GetFullResult() {
        if(HasError) {
            return ContourCheckEnum.Error;
        } else if(MainResult && !NeedToCheck) {
            return ContourCheckEnum.Yes;
        } else {
            return !MainResult && !NeedToCheck
                ? ContourCheckEnum.No
                : MainResult && NeedToCheck ? ContourCheckEnum.YesCheck : ContourCheckEnum.NoCheck;
        }
    }
}
