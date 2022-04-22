using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models
{
    internal class ClashModel
    {
        public Element MainElement { get; set; }
        public Element OtherElement { get; set; }
    }
}
