using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class CategoryContext {
    public Category SelectedCategory { get; set; }
    public bool IsMarkForTypes { get; set; }
}
