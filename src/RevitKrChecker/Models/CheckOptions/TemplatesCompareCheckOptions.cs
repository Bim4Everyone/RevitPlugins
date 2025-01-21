using System.Collections.Generic;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.CheckOptions {
    public class TemplatesCompareCheckOptions {
        public string CheckName { get; set; }
        public string TargetParamName { get; set; }
        public ParamLevel TargetParamLevel { get; set; }
        public ICheckRule CheckRule { get; set; }
        public string SourceParamName { get; set; }
        public ParamLevel SourceParamLevel { get; set; }
        public Dictionary<string, string> DictForCompare { get; set; }
    }
}
