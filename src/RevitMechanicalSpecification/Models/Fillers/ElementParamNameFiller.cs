using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNameFiller : ElementParamDefaultFiller {
        public ElementParamNameFiller(Document doc, SpecConfiguration config) : base(doc, config) 
        {
        }
    }
}
