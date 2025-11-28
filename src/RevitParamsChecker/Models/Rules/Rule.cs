namespace RevitParamsChecker.Models.Rules;

internal class Rule {
    public Rule() {
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public LogicalRule RootRule { get; set; }

    public Rule Copy() {
        throw new System.NotImplementedException(); // TODO
    }
}
