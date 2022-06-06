using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitClashDetective.Models.Interfaces {
    interface IRevitRuleCreator {
        FilterRule Create(StorageType storageType, ElementId paramId, object value);
    }
}
