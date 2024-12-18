namespace RevitKrChecker.Models.Rule {
    public class StartWithCheckRule : ICheckRule {

        public StartWithCheckRule() {
            CheckRuleName = "Начинается с";
            UnfulfilledRule = "не начинается";
        }

        public string CheckRuleName { get; }
        public string UnfulfilledRule { get; }

        public bool Check(string str1, string str2) => str1.StartsWith(str2);
    }
}
