using System.Collections.Generic;
using System;

using Autodesk.Revit.DB;
using System.Linq;

namespace RevitCorrectNamingCheck.Models
{
    public class WorksetInfo
    {
        public WorksetId Id { get; set; }
        public string Name { get; set; }
        public NameStatus WorksetNameStatus { get; set; }

        public WorksetInfo(WorksetId id, string name) {
            Id = id;
            Name = name;
        }
    }
}
