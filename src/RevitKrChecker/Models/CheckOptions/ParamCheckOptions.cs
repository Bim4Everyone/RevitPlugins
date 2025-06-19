using System.Collections.Generic;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckOptions;
public class ParamCheckOptions {
    public string CheckName { get; set; }
    public string TargetParamName { get; set; }
    public ParamLevel TargetParamLevel { get; set; }
    public ICheckRule CheckRule { get; set; }
    public List<string> TrueValues { get; set; }
}
