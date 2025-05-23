using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMirroredElements.Interfaces
{
    public interface ICategorySelectionService {
        List<Category> SelectCategories(List<Category> currentSelection);
    }
}
