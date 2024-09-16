using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal interface IElementParamFiller {
        void Fill(SpecificationElement specificationElement);
    }
}
