using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DevExpress.Xpf.Grid;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.ViewModels.Navigator;

namespace RevitOpeningPlacement.Views.Controls {
    internal class CustomGrid : GridControl {
        public static readonly DependencyProperty CustomSelectedItemsProperty =
            DependencyProperty.Register(nameof(CustomSelectedItems), typeof(ObservableCollection<OpeningViewModel>), typeof(CustomGrid));

        public ObservableCollection<OpeningViewModel> CustomSelectedItems {
            get => (ObservableCollection<OpeningViewModel>) GetValue(CustomSelectedItemsProperty);
            set => SetValue(CustomSelectedItemsProperty, value);
        }

        public CustomGrid() : base() {
            SelectionChanged += CustomGrid_SelectionChanged;
            ;
        }

        private void CustomGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e) {
            if(CustomSelectedItems == null) {
                CustomSelectedItems = new ObservableCollection<OpeningViewModel>();
            }

            var row = GetSelectedRowHandles()
                .Where(item => !IsGroupRowHandle(item))
                .Select(item => GetRow(item))
                .FirstOrDefault();

            if(row != null) {
                CustomSelectedItems.Clear();
                CustomSelectedItems.Add((OpeningViewModel) row);
                return;
            }

            InitializeCustomSelectedItems();
        }

        private void InitializeCustomSelectedItems() {
            var column = GetSelectedRowHandles()
                            .Where(item => IsGroupRowHandle(item))
                            .Select(item => GetRowLevelByRowHandle(item))
                            .Select(item => ((TableView) View).GroupedColumns[item])
                            .FirstOrDefault(item => item.FieldName.Equals(nameof(OpeningViewModel.ParentId)));

            var value = GetSelectedRowHandles()
                .Where(item => IsGroupRowHandle(item))
                .Select(item => GetGroupRowValue(item))
                .FirstOrDefault();

            if(column != null && value != null && (int) value != 0) {
                var handle = GetSelectedRowHandles()
                .First(item => IsGroupRowHandle(item));

                CustomSelectedItems.Clear();
                CustomSelectedItems.AddRange(
                     Enumerable.Range(0, GetChildRowCount(handle))
                     .Select(item => GetChildRowHandle(handle, item))
                     .Select(item => GetRow(item))
                     .OfType<OpeningViewModel>()
                    );
            }
        }
    }
}
