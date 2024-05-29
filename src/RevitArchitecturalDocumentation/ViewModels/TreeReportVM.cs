using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;

namespace RevitArchitecturalDocumentation.ViewModels {
    class TreeReportVM : BaseViewModel {

        private ICollectionView _treeView;
        private bool _showImportant = true;
        private string _stringForUnimportantNodes = string.Empty;
        private ObservableCollection<TreeReportNode> _data = new ObservableCollection<TreeReportNode>();

        /// <summary>
        /// Модель вида древовидного отчета. Строкой для неважных узлов
        /// </summary>
        /// <param name="treeReportNodes"></param>
        /// <param name="stringForUnimportantNodes"></param>
        public TreeReportVM(ObservableCollection<TreeReportNode> treeReportNodes, string stringForUnimportantNodes) {
            Data = treeReportNodes;
            StringForUnimportantNodes = stringForUnimportantNodes;
            SetTreeViewFilter();

            RefreshTreeViewCommand = RelayCommand.Create(RefreshTreeView);
        }

        public ICommand RefreshTreeViewCommand { get; }

        /// <summary>
        /// Флаг для окна, работающий на включение фильтра по отчету
        /// </summary>
        public bool ShowImportant {
            get => _showImportant;
            set => this.RaiseAndSetIfChanged(ref _showImportant, value);
        }

        /// <summary>
        /// Фрагмент фразы, которая будет указывать, что это неважный узел
        /// </summary>
        public string StringForUnimportantNodes {
            get => _stringForUnimportantNodes;
            set => this.RaiseAndSetIfChanged(ref _stringForUnimportantNodes, value);
        }

        /// <summary>
        /// Содержимое отчета для представления в виде дерева
        /// </summary>
        public ObservableCollection<TreeReportNode> Data {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }

        /// <summary>
        /// Фильтрация отчета по первому уровню на основе наличия фразы неважного узла StringForUnimportantNodes
        /// </summary>
        private void SetTreeViewFilter() {
            _treeView = CollectionViewSource.GetDefaultView(Data);
            _treeView.Filter = item => ShowImportant ? ((TreeReportNode) item).Name.IndexOf(StringForUnimportantNodes, StringComparison.OrdinalIgnoreCase) == -1 : true;
        }

        /// <summary>
        /// Обновляет элемент вида, связанный с коллекцией при изменении выбора фильтрации списка в интерфейсе
        /// </summary>
        private void RefreshTreeView() {
            _treeView.Refresh();
        }
    }
}
