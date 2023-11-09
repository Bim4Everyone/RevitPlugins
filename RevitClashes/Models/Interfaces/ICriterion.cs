using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    interface ICriterion {
        IFilterGenerator FilterGenerator { get; set; }
        IFilterGenerator Generate(Document doc);
        IEnumerable<IFilterableValueProvider> GetProviders();
    }
}
