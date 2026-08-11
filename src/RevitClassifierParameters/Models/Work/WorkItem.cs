namespace RevitClassifierParameters.Models.Work;

internal class WorkItem {
    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public WorkGroup ParentWorkGroup { get; set; }
}
