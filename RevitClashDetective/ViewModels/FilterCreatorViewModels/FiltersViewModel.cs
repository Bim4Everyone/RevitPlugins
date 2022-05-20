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
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FiltersViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _config;
        private ObservableCollection<FilterViewModel> _filters;
        private FilterViewModel _selectedFilter;
        private string _errorText;

        public FiltersViewModel(RevitRepository revitRepository, FiltersConfig config) {
            _revitRepository = revitRepository;
            _config = config;
            CreateCommand = new RelayCommand(Create);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            RenameCommand = new RelayCommand(Rename, CanRename);
            SaveCommand = new RelayCommand(Save, CanSave);


            Filters = new ObservableCollection<FilterViewModel>();

            InitializeFilters();
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand CreateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RenameCommand { get; }

        public ICommand SaveCommand { get; }

        public FilterViewModel SelectedFilter {
            get => _selectedFilter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilter, value);
        }

        public ObservableCollection<FilterViewModel> Filters {
            get => _filters;
            set => this.RaiseAndSetIfChanged(ref _filters, value);
        }

        public IEnumerable<Filter> GetFilters() {
            return Filters.Select(item => item.GetFilter());
        }

        private void InitializeFilters() {
            foreach(var filter in _config.Filters) {
                filter.Set.SetRevitRepository(_revitRepository);
                Filters.Add(new FilterViewModel(_revitRepository, filter));
            }
        }

        private void Create(object p) {
            var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name));
            var view = new FilterNameView() { DataContext = newFilterName, Owner = p as Window };
            if(view.ShowDialog() == true) {
                var newFilter = new FilterViewModel(_revitRepository) { Name = newFilterName.Name };
                Filters.Add(newFilter);

                Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
                SelectedFilter = newFilter;
            }
        }

        private void Delete(object p) {
            var taskDialog = new TaskDialog("Revit");
            taskDialog.MainContent = $"Удалить фильтр \"{SelectedFilter.Name}\"?";
            taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            if(taskDialog.Show() == TaskDialogResult.Yes) {
                Filters.Remove(SelectedFilter);
                SelectedFilter = Filters.FirstOrDefault();
            }
        }

        private bool CanDelete(object p) {
            return SelectedFilter != null;
        }

        private void Rename(object p) {
            var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name), SelectedFilter.Name);
            var view = new FilterNameView() { DataContext = newFilterName, Owner = p as Window };
            if(view.ShowDialog() == true) {
                SelectedFilter.Name = newFilterName.Name;
            }
        }

        private bool CanRename(object p) {
            return SelectedFilter != null;
        }

        private void Save(object p) {
            var filtersConfig = FiltersConfig.GetFiltersConfig();
            filtersConfig.Filters = GetFilters().ToList();
            filtersConfig.SaveProjectConfig();

            //foreach(var filter in filters) {
            //    filter

            //    filter.Save();
            //    var readFilter = filter.Read();
            //    readFilter.RevitRepository = _revitRepository;
            //    readFilter.Set.SetRevitRepository(_revitRepository);
            //    readFilter.CreateRevitFilter();
            //}
        }

        private bool CanSave(object p) {
            if(Filters.Any(item => item.Set.IsEmpty())) {
                ErrorText = "Все поля в критериях фильтрации должны быть заполнены.";
                return false;
            }

            ErrorText = string.Empty;
            return true;
        }
    }
}