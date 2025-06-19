namespace RevitKrChecker.Models.Interfaces;
public interface ICheckRule {
    string CheckRuleName { get; }
    string UnfulfilledRule { get; }

    bool Check(string value1, string value2);
}
