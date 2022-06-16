using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.ClashDetective.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class SelectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _filterConfig;
        private List<FileViewModel> _files;
        private List<IProviderViewModel> _providers;
        private string _selectedPoviders;
        private string _selectedFiles;
        private bool? _isAllFilesSelected;

        public SelectionViewModel(RevitRepository revitRepository, FiltersConfig filterConfig) {
            _revitRepository = revitRepository;
            _filterConfig = filterConfig;
            InitializeFiles();
            InitializeProviders();

            SelectProvidersCommand = new RelayCommand(SelectProviders);
            SelectFilesCommand = new RelayCommand(SelectFiles);
        }

        public string SelectedPoviders {
            get => _selectedPoviders;
            set => this.RaiseAndSetIfChanged(ref _selectedPoviders, value);
        }

        public string SelectedFiles {
            get => _selectedFiles;
            set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
        }

        public ICommand SelectProvidersCommand { get; }
        public ICommand SelectFilesCommand { get; }

        public List<FileViewModel> Files {
            get => _files;
            set => this.RaiseAndSetIfChanged(ref _files, value);
        }

        public bool? IsAllFilesSelected {
            get => _isAllFilesSelected;
            set => this.RaiseAndSetIfChanged(ref _isAllFilesSelected, value);
        }

        public List<IProviderViewModel> Providers {
            get => _providers;
            set => this.RaiseAndSetIfChanged(ref _providers, value);
        }


        public IEnumerable<IProvider> GetProviders() {
            return Files.Where(item => item.IsSelected)
                .SelectMany(item => Providers.Where(p => p.IsSelected)
                    .Select(p => p.GetProvider(item.Doc, item.Transform)));
        }

        public void SelectProviders(object p) {
            SelectedPoviders = string.Join(", ",
               Providers
               .Where(item => item.IsSelected)
               .Select(item => item.Name));

        }

        public void SelectFiles(object p) {
            SelectedFiles = string.Join(", ",
               Files
               .Where(item => item.IsSelected)
               .Select(item => item.Name));

            if(Files.All(item => item.IsSelected)) {
                IsAllFilesSelected = true;
                return;
            }
            if(Files.Any(item => item.IsSelected)) {
                IsAllFilesSelected = null;
                return;
            }
            IsAllFilesSelected = false;
        }

        private void InitializeFiles() {
            Files = _revitRepository.GetRevitLinkInstances()
                .Select(item => new FileViewModel(_revitRepository, item.GetLinkDocument(), item.GetTransform()))
                .ToList();
            Files.Add(new FileViewModel(_revitRepository, _revitRepository.Doc, Transform.Identity));

            IsAllFilesSelected = false;
        }

        private void InitializeProviders() {
            Providers = new List<IProviderViewModel>(
                _filterConfig.Filters
                .Select(item => new FilterProviderViewModel(_revitRepository, item)));
        }
    }
}
