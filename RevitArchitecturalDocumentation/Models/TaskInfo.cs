using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models
{
    class TaskInfo
    {
        public TaskInfo() {

        }

        public string StartLevelNumber { get; set; }
        public string EndLevelNumber { get; set; }
        public Element SelectedVisibilityScope { get; set; }
    }
}
