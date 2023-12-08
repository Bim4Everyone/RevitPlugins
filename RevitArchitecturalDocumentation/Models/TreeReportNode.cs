using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

using Autodesk.AdvanceSteel.StructuralAnalysis;
using Autodesk.Revit.UI;

namespace RevitArchitecturalDocumentation.Models
{
    class TreeReportNode
    {
        public TreeReportNode(TreeReportNode parent) {
            Parent = parent;
        }

        public TreeReportNode Parent { get; set; }
        public string Name { get; set; }
        public ObservableCollection<TreeReportNode> Nodes { get; set; } = new ObservableCollection<TreeReportNode>();

        public void AddNodeWithName(string name) {
            Nodes.Add(new TreeReportNode(this) { Name = name });
        }

        public void FindInChildName(string text) {
            foreach (TreeReportNode node in Nodes) { 
                if(node.Name.Contains(text)) {
                    WriteTextInParentName(text);
                }
                node.FindInChildName(text);
            }
        }

        public void WriteTextInParentName(string text) {

            if(!Name.Contains(text)) {
                Name = text + Name;
            }
            if(Parent != null && !Parent.Name.Contains(text)) {
                Parent.Name = text + Parent.Name;
                Parent.WriteTextInParentName(text);
            }
        }
    }
}
