using dosymep.SimpleServices;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckRule {
    public class ContainsCheckRule : ICheckRule {

        public ContainsCheckRule(ILocalizationService localizationService) {
            CheckRuleName = localizationService.GetLocalizedString("ReportWindow.Contains");
            UnfulfilledRule = localizationService.GetLocalizedString("ReportWindow.NotContains");
        }

        public string CheckRuleName { get; }
        public string UnfulfilledRule { get; }

        public bool Check(string str1, string str2) => str1 != null && str2 != null && str1.Contains(str2);
    }
}
