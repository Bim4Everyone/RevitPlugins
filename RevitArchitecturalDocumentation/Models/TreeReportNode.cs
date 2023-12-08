using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

using Autodesk.AdvanceSteel.StructuralAnalysis;

namespace RevitArchitecturalDocumentation.Models
{
    class TreeReportNode
    {
        public string Name { get; set; }
        public ObservableCollection<TreeReportNode> Nodes { get; set; } = new ObservableCollection<TreeReportNode>();

        public void AddNodeWithName(string name) {
            Nodes.Add(new TreeReportNode { Name = name });
        }
    }
}
