using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckOptions;
public class CompareCheckOptions {
    public string CheckName { get; set; }
    public string TargetParamName { get; set; }
    public ParamLevel TargetParamLevel { get; set; }
    public ICheckRule CheckRule { get; set; }
    public string SourceParamName { get; set; }
    public ParamLevel SourceParamLevel { get; set; }
}
