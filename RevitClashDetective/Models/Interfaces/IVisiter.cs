using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitClashDetective.Models.Interfaces {
    interface IVisiter {
        FilterRule Create(ElementId paramId, int value);
        FilterRule Create(ElementId paramId, double value);
        FilterRule Create(ElementId paramId, string value);
        FilterRule Create(ElementId paramId, ElementId value);
    }
}