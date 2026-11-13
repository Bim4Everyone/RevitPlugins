namespace RevitParamsChecker.Models.Rules;

internal class Rule {
    public Rule() {
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public LogicalRule RootRule { get; set; } = new LogicalRule();

    public Rule Copy() {
        return new Rule() {
            Name = Name,
            Description = Description,
            RootRule = (LogicalRule) RootRule.Copy()
        };
    }
}
