using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models.Options {
    internal sealed class SheetOptions {
        public SheetOptions() { }

        public bool WorkWithSheets { get; set; }
        public FamilySymbol SelectedTitleBlock { get; set; }
        public string SheetNamePrefix { get; set; }
    }
}
