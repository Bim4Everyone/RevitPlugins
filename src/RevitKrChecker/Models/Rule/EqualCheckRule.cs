namespace RevitKrChecker.Models.Rule {
    public class EqualCheckRule : ICheckRule {

        public EqualCheckRule() {
            CheckRuleName = "Равно";
            UnfulfilledRule = "не равно";
        }

        public string CheckRuleName { get; }
        public string UnfulfilledRule { get; }

        public bool Check(string str1, string str2) => str1.Equals(str2);
    }
}
