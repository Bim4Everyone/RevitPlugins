using System;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterModel {
    internal class Criterion : ICriterion {
        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }
        [JsonIgnore]
        public IFilterGenerator FilterGenerator { get; set; }
        public virtual void SetRevitRepository(RevitRepository revitRepository) {
            RevitRepository = revitRepository;
        }

        public virtual IFilterGenerator Generate(Document doc) {
            throw new NotImplementedException();
        }
    }
}
