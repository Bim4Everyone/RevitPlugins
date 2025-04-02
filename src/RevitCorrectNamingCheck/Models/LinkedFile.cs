using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitCorrectNamingCheck.Helpers;

namespace RevitCorrectNamingCheck.Models
{
    internal class LinkedFile {
        public ElementId Id { get; set; }
        public string Name { get; set; }
        public NameStatus FileNameStatus { get; set; }
        public WorksetInfo TypeWorkset { get; set; }
        public WorksetInfo InstanceWorkset { get; set; }

        public LinkedFile(ElementId id, string name, WorksetInfo typeWorkset, WorksetInfo instanceWorkset) {
            Id = id;
            Name = name;
            TypeWorkset = typeWorkset;
            InstanceWorkset = instanceWorkset;
        }
    }
}
