using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Views;

namespace RevitArchitecturalDocumentation.ViewModels
{
    class TreeReportVM : BaseViewModel {

        private ObservableCollection<TreeReportNode> _data = new ObservableCollection<TreeReportNode>();

        public TreeReportVM(ObservableCollection<TreeReportNode> treeReportNodes) {
            Data = treeReportNodes;
        }

        public ObservableCollection<TreeReportNode> Data {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }
    }
}
