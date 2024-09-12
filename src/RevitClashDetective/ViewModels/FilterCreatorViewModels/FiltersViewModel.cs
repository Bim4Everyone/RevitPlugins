using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.SearchSet;
using RevitClashDetective.ViewModels.Services;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FiltersViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _config;
        private ObservableCollection<FilterViewModel> _filters;
        private string _errorText;
        private string _messageText;
        private DispatcherTimer _timer;
        private FilterViewModel _selectedFilter;

        public FiltersViewModel(RevitRepository revitRepository, FiltersConfig config) {
            _revitRepository = revitRepository;
            _config = config;

            InitializeFilters();
            InitializeTimer();

            SelectedFilterChangedCommand = RelayCommand.Create(SelectedFilterChanged, CanSelectedFilterChanged);
            CreateCommand = RelayCommand.Create<Window>(Create);
            DeleteCommand = RelayCommand.Create(Delete, CanDelete);
            RenameCommand = RelayCommand.Create<Window>(Rename, CanRename);
            SaveCommand = RelayCommand.Create(Save, CanSave);
            SaveAsCommand = RelayCommand.Create(SaveAs, CanSave);
            LoadCommand = RelayCommand.Create(Load);
            CheckSearchSetCommand = RelayCommand.Create(CheckSearchSet, CanSave);

            SelectedFilter = Filters.FirstOrDefault();
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string MessageText {
            get => _messageText;
            set => RaiseAndSetIfChanged(ref _messageText, value);
        }

        public ICommand CreateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand SelectedFilterChangedCommand { get; }

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand CheckSearchSetCommand { get; }


        public FilterViewModel SelectedFilter {
            get => _selectedFilter;
            set => RaiseAndSetIfChanged(ref _selectedFilter, value);
        }

        public ObservableCollection<FilterViewModel> Filters {
            get => _filters;
            set => RaiseAndSetIfChanged(ref _filters, value);
        }

        public IEnumerable<Filter> GetFilters() {
            return Filters.Select(item => item.GetFilter());
        }

        private void InitializeFilters() {
            Filters = new ObservableCollection<FilterViewModel>(InitializeFilters(_config));
        }

        private IEnumerable<FilterViewModel> InitializeFilters(FiltersConfig config) {
            foreach(var filter in config.Filters.OrderBy(item => item.Name)) {
                filter.RevitRepository = _revitRepository;
                filter.Set.SetRevitRepository(_revitRepository);
                yield return new FilterViewModel(_revitRepository, filter);
            }
        }

        private void SelectedFilterChanged() {
            SelectedFilter?.InitializeFilter();
        }

        private bool CanSelectedFilterChanged() {
            return SelectedFilter != null;
        }

        private void Create(Window p) {
            var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name));
            var view = new FilterNameView() { DataContext = newFilterName, Owner = p };
            if(view.ShowDialog() == true) {
                var newFilter = new FilterViewModel(_revitRepository) { Name = newFilterName.Name, IsInitialized = true };
                Filters.Add(newFilter);

                Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
                SelectedFilter = newFilter;
            }
        }

        private void Delete() {
            var dialog = GetPlatformService<IMessageBoxService>();
            if(dialog.Show($"Удалить фильтр \"{SelectedFilter.Name}\"?", "BIM", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes) {
                Filters.Remove(SelectedFilter);
                SelectedFilter = Filters.FirstOrDefault();
            }
        }

        private bool CanDelete() {
            return SelectedFilter != null;
        }

        private void Rename(Window p) {
            var newFilterName = new FilterNameViewModel(Filters.Select(f => f.Name), SelectedFilter.Name);
            var view = new FilterNameView() { DataContext = newFilterName, Owner = p };
            if(view.ShowDialog() == true) {
                SelectedFilter.Name = newFilterName.Name;
                Filters = new ObservableCollection<FilterViewModel>(Filters.OrderBy(item => item.Name));
                SelectedFilter = Filters.FirstOrDefault(item => item.Name.Equals(newFilterName.Name, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        private bool CanRename(Window p) {
            return SelectedFilter != null;
        }

        private void Save() {
            var revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
            var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
            filtersConfig.Filters = GetFilters().ToList();
            filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;
            filtersConfig.SaveProjectConfig();
            MessageText = "Поисковые наборы успешно сохранены";
            RefreshMessage();
        }

        private void SaveAs() {
            var revitFilePath = Path.Combine(_revitRepository.GetObjectName(), _revitRepository.GetDocumentName());
            var filtersConfig = FiltersConfig.GetFiltersConfig(revitFilePath, _revitRepository.Doc);
            filtersConfig.Filters = GetFilters().ToList();
            filtersConfig.RevitVersion = ModuleEnvironment.RevitVersion;

            ConfigSaverService cs = new ConfigSaverService(_revitRepository);
            cs.Save(filtersConfig);
            MessageText = "Поисковые наборы успешно сохранены";
            RefreshMessage();
        }

        private bool CanSave() {
            if(SelectedFilter == null || !SelectedFilter.IsInitialized)
                return false;

            if(Filters.Any(item => item.Set.IsEmpty())) {
                ErrorText = $"Все поля в поисковом наборе \"{Filters.FirstOrDefault(item => item.Set.IsEmpty())?.Name}\" должны быть заполнены.";
                return false;
            }

            ErrorText = Filters.FirstOrDefault(item => item.Set.GetErrorText() != null)?.Set?.GetErrorText();
            if(!string.IsNullOrEmpty(ErrorText)) {
                return false;
            }

            return true;
        }

        private void Load() {
            var cl = new ConfigLoaderService(_revitRepository);
            var config = cl.Load<FiltersConfig>();
            cl.CheckConfig(config);

            var newFilters = InitializeFilters(config).ToList();
            var nameResolver = new NameResolver<FilterViewModel>(Filters, newFilters);
            Filters = new ObservableCollection<FilterViewModel>(nameResolver.GetCollection());
            MessageText = "Файл поисковых наборов успешно загружен";
            RefreshMessage();
        }

        private void CheckSearchSet() {
            Save();
            var filter = SelectedFilter.GetFilter();
            var vm = new SearchSetsViewModel(_revitRepository, filter);
            var view = new SearchSetView() { DataContext = vm };
            view.Show();
        }

        private void InitializeTimer() {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 3);
            _timer.Tick += (s, a) => { MessageText = null; _timer.Stop(); };
        }

        private void RefreshMessage() {
            _timer.Start();
        }
    }
}
