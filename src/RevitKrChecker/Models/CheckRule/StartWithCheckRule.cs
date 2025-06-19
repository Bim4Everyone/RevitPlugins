using dosymep.SimpleServices;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckRule;
public class StartWithCheckRule : ICheckRule {

    public StartWithCheckRule(ILocalizationService localizationService) {
        CheckRuleName = localizationService.GetLocalizedString("ReportWindow.StartsWith");
        UnfulfilledRule = localizationService.GetLocalizedString("ReportWindow.NotStartsWith");
    }

    public string CheckRuleName { get; }
    public string UnfulfilledRule { get; }

    public bool Check(string str1, string str2) => str1 != null && str2 != null && str1.StartsWith(str2);
}
