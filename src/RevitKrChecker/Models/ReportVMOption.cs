using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models {
    public class ReportVMOption {
        internal List<Element> Elements { get; set; }
        internal List<ICheck> StoppingChecks { get; set; }
        internal List<ICheck> NonStoppingChecks { get; set; }
        internal RevitRepository RepositoryOfRevit { get; set; }
        internal PluginConfig ConfigOfPlugin { get; set; }
        internal ILocalizationService LocalizationService { get; set; }
    }
}
