using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Views;

namespace RevitArchitecturalDocumentation.ViewModels
{
    class TreeReportVM : BaseViewModel {

        private ICollectionView _treeView;
        private bool _showImportant = true;
        private ObservableCollection<TreeReportNode> _data = new ObservableCollection<TreeReportNode>();

        public TreeReportVM(ObservableCollection<TreeReportNode> treeReportNodes) {
            Data = treeReportNodes;
            SetTreeViewFilter();

            RefreshTreeViewCommand = RelayCommand.Create(RefreshTreeView);
        }

        public ICommand RefreshTreeViewCommand { get; }


        public bool ShowImportant {
            get => _showImportant;
            set => this.RaiseAndSetIfChanged(ref _showImportant, value);
        }

        public ObservableCollection<TreeReportNode> Data {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }

        private void SetTreeViewFilter() {
            // Организуем фильтрацию списка категорий
            _treeView = CollectionViewSource.GetDefaultView(Data);
            _treeView.Filter = item => ShowImportant ? ((TreeReportNode) item).Name.IndexOf("  ~  ", StringComparison.OrdinalIgnoreCase) == -1 : true;
        }

        private void RefreshTreeView() {
            _treeView.Refresh();
        }
    }
}
