using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal interface IElementParamFiller {
        void Fill(Element element, FamilyInstance familyInstance, int count = 0);
    }
}
