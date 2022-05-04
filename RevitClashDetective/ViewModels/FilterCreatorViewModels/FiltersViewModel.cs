using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FiltersViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<FilterViewModel> _filters;
        private FilterViewModel _selectedFilter;

        public FiltersViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            CreateCommand = new RelayCommand(Create);
            DeleteCommand = new RelayCommand(Delete);
            RenameCommand = new RelayCommand(Rename);
            Filters = new ObservableCollection<FilterViewModel>();
        }

        public ICommand CreateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand DataContextChangedCommand { get; }

        public FilterViewModel SelectedFilter {
            get => _selectedFilter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilter, value);
        }

        public ObservableCollection<FilterViewModel> Filters {
            get => _filters;
            set => this.RaiseAndSetIfChanged(ref _filters, value);
        }

        private void Create(object p) {
            if(p is Window window) {
                var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name));
                var view = new FilterNameView() { DataContext = newFilterName, Owner = window };
                var result = view.ShowDialog();
                if(result.HasValue && result.Value) {
                    var newFilter = new FilterViewModel(_revitRepository);
                    newFilter.Name = newFilterName.Name;
                    Filters.Add(newFilter);
                    Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
                    SelectedFilter = Filters.FirstOrDefault(item => item.Name == newFilter.Name);
                }
            }

        }

        private void Delete(object p) {
            if(SelectedFilter != null) {
                var taskDialog = new TaskDialog("Revit");
                taskDialog.MainContent = $"Удалить фильтр \"{SelectedFilter.Name}\"?";
                taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                if(taskDialog.Show() == TaskDialogResult.Yes) {
                    Filters.Remove(SelectedFilter);
                }

            }
        }

        private void Rename(object p) {
            if(p is Window window) {
                if(SelectedFilter != null) {
                    var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name), SelectedFilter.Name);
                    var view = new FilterNameView() { DataContext = newFilterName, Owner = window };
                    var result = view.ShowDialog();
                    if(result.HasValue && result.Value) {
                        SelectedFilter.Name = newFilterName.Name;
                    }
                }
            }
        }
    }
}