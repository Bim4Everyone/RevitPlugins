using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;

namespace RevitClashDetective.Models {
    internal class SetProvider : IProvider {
        public List<Element> GetElements(Document doc) {
            throw new NotImplementedException();
        }

        public List<Solid> GetSolids(Document doc, Transform transform) {
            throw new NotImplementedException();
        }
    }
}
