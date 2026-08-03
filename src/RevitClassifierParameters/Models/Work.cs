namespace RevitClassifierParameters.Models;

public class Work {
    public string Code { get; set; }
    public string Name { get; set; }
    public WorkGroup ParentWorkGroup { get; set; }
}
