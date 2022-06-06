using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models.Clashes {
    internal class ClashModel {
        private readonly RevitRepository _revitRepository;

        public ClashModel(RevitRepository revitRepository, Element mainElement, Element otherElement) {
            _revitRepository = revitRepository;

            MainElement = new ElementModel(_revitRepository, mainElement);
            OtherElement = new ElementModel(_revitRepository, otherElement);
        }

        public ClashModel() {}
        public bool IsSolved { get; set; }
        public ElementModel MainElement { get; set; }
        public ElementModel OtherElement { get; set; }

    }
}
