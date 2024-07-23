using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNumberFiller : ElementParamFiller {

        //private readonly double _insulationStock;
        //private readonly double _ductAndPipeStock;

        public ElementParamNumberFiller(string toParamName, string fromParamName) : base(toParamName, fromParamName) {
        }

        public override void SetParamValue(Element element) {
            throw new NotImplementedException();
        }
    }
}
