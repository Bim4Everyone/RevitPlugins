using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckRule {
    public class EqualCheckRule : ICheckRule {

        public EqualCheckRule() {
            CheckRuleName = "Равно";
            UnfulfilledRule = "не равно";
        }

        public string CheckRuleName { get; }
        public string UnfulfilledRule { get; }

        public bool Check(string str1, string str2) => str1 != null && str2 != null && str1.Equals(str2);
    }
}
