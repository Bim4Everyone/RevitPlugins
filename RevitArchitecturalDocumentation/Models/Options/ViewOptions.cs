using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models.Options {
    internal class ViewOptions {
        public ViewOptions() {}

        public bool WorkWithViews { get; set; }
        public ViewFamilyType SelectedViewFamilyType { get; set; }
        public ElementType SelectedViewportType { get; set; }
        public string ViewNamePrefix { get; set; }
    }
}
