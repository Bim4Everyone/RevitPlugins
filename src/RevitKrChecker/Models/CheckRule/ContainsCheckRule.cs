using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckRule {
    public class ContainsCheckRule : ICheckRule {

        public ContainsCheckRule() {
            CheckRuleName = "Содержит";
            UnfulfilledRule = "не содержит";
        }

        public string CheckRuleName { get; }
        public string UnfulfilledRule { get; }

        public bool Check(string str1, string str2) => str1 != null && str2 != null && str1.Contains(str2);
    }
}
