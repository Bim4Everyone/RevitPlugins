namespace RevitDeclarations.Models {
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
            } else if(!MainResult && !NeedToCheck) {
                return ContourCheckEnum.No;
            } else if(MainResult && NeedToCheck) {
                return ContourCheckEnum.YesCheck;
            } else {
                return ContourCheckEnum.NoCheck;
            }
        }
    }
}
