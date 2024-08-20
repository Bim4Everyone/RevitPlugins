using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.ClashDetective.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class SelectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _filterConfig;
        private readonly SelectionConfig _selectionConfig;
        private ObservableCollection<FileViewModel> _files;
        private List<IProviderViewModel> _providers;
        private bool? _isAllFilesSelected;
        private bool? _isAllProvidersSelected;

        public SelectionViewModel(RevitRepository revitRepository, FiltersConfig filterConfig, SelectionConfig selectionConfig = null) {
            _revitRepository = revitRepository;
            _filterConfig = filterConfig;
            _selectionConfig = selectionConfig;
            if(selectionConfig == null) {
                InitializeFiles();
                InitializeProviders();
            } else {
                InitializeFiles(selectionConfig);
                InitializeProviders(selectionConfig);
            }
        }


        public ICommand SelectProvidersCommand { get; }
        public ICommand SelectFilesCommand { get; }

        public ObservableCollection<FileViewModel> Files {
            get => _files;
            set => this.RaiseAndSetIfChanged(ref _files, value);
        }

        public List<object> SelectedFileObjects { get; set; } = new List<object>();
        public IEnumerable<FileViewModel> SelectedFiles => SelectedFileObjects?.OfType<FileViewModel>() ?? Enumerable.Empty<FileViewModel>();
        public List<object> SelectedProvidersObjects { get; set; } = new List<object>();
        public IEnumerable<IProviderViewModel> SelectedProviders => SelectedProvidersObjects?.OfType<IProviderViewModel>() ?? Enumerable.Empty<IProviderViewModel>();

        public bool? IsAllFilesSelected {
            get => _isAllFilesSelected;
            set => this.RaiseAndSetIfChanged(ref _isAllFilesSelected, value);
        }

        public bool? IsAllProvidersSelected {
            get => _isAllProvidersSelected;
            set => this.RaiseAndSetIfChanged(ref _isAllProvidersSelected, value);
        }

        public List<IProviderViewModel> Providers {
            get => _providers;
            set => this.RaiseAndSetIfChanged(ref _providers, value);
        }


        public IEnumerable<IProvider> GetProviders() {
            return SelectedFiles
                .SelectMany(item => SelectedProviders
                    .Select(p => p.GetProvider(item.Doc, item.Transform)));
        }

        public SelectionConfig GetCheckSettings() {
            return new SelectionConfig() {
                Files = SelectedFiles.Select(item => item.Name).ToList(),
                Filters = SelectedProviders.Select(item => item.Name).ToList()
            };
        }

        public string GetMissedFiles() {
            var missedFiles = _selectionConfig.Files
               .Where(item => !Files.Any(p => p.Name.Equals(item)))
               .ToList();
            return string.Join(",", missedFiles);
        }

        public string GetMissedFilters() {
            var missedFilters = _selectionConfig.Filters
               .Where(item => !Providers.Any(p => p.Name.Equals(item)))
               .ToList();
            return string.Join(",", missedFilters);
        }

        private void InitializeFiles() {
            Files = new ObservableCollection<FileViewModel>(_revitRepository.GetRevitLinkInstances()
                .Select(item => new FileViewModel(item.GetLinkDocument(), item.GetTransform())));
            Files.Add(new FileViewModel(_revitRepository.Doc, Transform.Identity));

            IsAllFilesSelected = false;
        }

        private void InitializeFiles(SelectionConfig selectionConfig) {
            InitializeFiles();

            SelectedFileObjects = Files
                .Where(item => selectionConfig.Files.Any(f => f.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                .Cast<object>()
                .ToList();
        }

        private void InitializeProviders() {
            Providers = new List<IProviderViewModel>(
                _filterConfig.Filters
                .Select(item => new FilterProviderViewModel(_revitRepository, item)));

            IsAllProvidersSelected = false;
        }

        private void InitializeProviders(SelectionConfig selectionConfig) {
            InitializeProviders();
            SelectedProvidersObjects = Providers
                .Where(item => selectionConfig.Filters.Any(f => f.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                .Cast<object>()
                .ToList();
        }
    }
}
