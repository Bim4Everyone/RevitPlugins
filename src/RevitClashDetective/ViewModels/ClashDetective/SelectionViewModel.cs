using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Views;

using Wpf.Ui;

namespace RevitClashDetective.ViewModels.ClashDetective;
internal class SelectionViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly FiltersConfig _filterConfig;
    private readonly ILocalizationService _localizationService;
    private readonly IContentDialogService _contentDialogService;
    private readonly SelectionConfig _selectionConfig;
    private ObservableCollection<FileViewModel> _allFiles = [];
    private ObservableCollection<FileViewModel> _selectedFiles = [];
    private ObservableCollection<FilterProviderViewModel> _allProviders = [];
    private ObservableCollection<FilterProviderViewModel> _selectedProviders = [];

    public SelectionViewModel(RevitRepository revitRepository,
        FiltersConfig filterConfig,
        ILocalizationService localizationService,
        IContentDialogService contentDialogService,
        SelectionConfig selectionConfig = null) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _filterConfig = filterConfig ?? throw new ArgumentNullException(nameof(filterConfig));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _contentDialogService = contentDialogService ?? throw new ArgumentNullException(nameof(contentDialogService));
        _selectionConfig = selectionConfig;
        if(selectionConfig == null) {
            InitializeFiles();
            InitializeProviders();
        } else {
            InitializeFiles(selectionConfig);
            InitializeProviders(selectionConfig);
        }

        SelectFilesCommand = RelayCommand.CreateAsync(SelectFilesAsync, CanSelectFiles);
        SelectProvidersCommand = RelayCommand.CreateAsync(SelectProvidersAsync, CanSelectProviders);
    }


    public IAsyncCommand SelectProvidersCommand { get; }

    public IAsyncCommand SelectFilesCommand { get; }

    public ObservableCollection<FileViewModel> AllFiles {
        get => _allFiles;
        private set => RaiseAndSetIfChanged(ref _allFiles, value);
    }

    public ObservableCollection<FileViewModel> SelectedFiles {
        get => _selectedFiles;
        set => RaiseAndSetIfChanged(ref _selectedFiles, value);
    }

    public ObservableCollection<FilterProviderViewModel> AllProviders {
        get => _allProviders;
        private set => RaiseAndSetIfChanged(ref _allProviders, value);
    }

    public ObservableCollection<FilterProviderViewModel> SelectedProviders {
        get => _selectedProviders;
        set => RaiseAndSetIfChanged(ref _selectedProviders, value);
    }

    public IEnumerable<IProvider> GetProviders() {
        return SelectedFiles
            .SelectMany(item => SelectedProviders
                .Select(p => p.GetProvider(item.Doc, item.Transform)));
    }

    public SelectionConfig GetCheckSettings() {
        return new SelectionConfig() {
            Files = [.. SelectedFiles.Select(item => item.Name)],
            Filters = [.. SelectedProviders.Select(item => item.Name)]
        };
    }

    public string GetMissedFiles() {
        string[] missedFiles = _selectionConfig.Files
           .Where(item => !AllFiles.Any(p => p.Name.Equals(item)))
           .ToArray();
        return string.Join(",", missedFiles);
    }

    public string GetMissedFilters() {
        string[] missedFilters = _selectionConfig.Filters
           .Where(item => !AllProviders.Any(p => p.Name.Equals(item)))
           .ToArray();
        return string.Join(",", missedFilters);
    }

    private void InitializeFiles() {
        AllFiles = new ObservableCollection<FileViewModel>(_revitRepository.GetRevitLinkInstances()
            .Select(item => new FileViewModel(item.GetLinkDocument(), item.GetTransform()))) {
            new(_revitRepository.Doc, Transform.Identity)
        };
    }

    private void InitializeFiles(SelectionConfig selectionConfig) {
        InitializeFiles();

        SelectedFiles = new ObservableCollection<FileViewModel>(AllFiles
            .Where(item => selectionConfig.Files.Any(
                f => f.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))));
    }

    private void InitializeProviders() {
        AllProviders = new ObservableCollection<FilterProviderViewModel>(
            _filterConfig.Filters
            .Select(item => new FilterProviderViewModel(item)));
    }

    private void InitializeProviders(SelectionConfig selectionConfig) {
        InitializeProviders();
        SelectedProviders = new ObservableCollection<FilterProviderViewModel>(AllProviders
            .Where(item => selectionConfig.Filters.Any(
                f => f.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))));
    }

    private async Task SelectFilesAsync() {
        var vm = new SelectableNamesViewModel(AllFiles, SelectedFiles) {
            Title = _localizationService.GetLocalizedString("SelectableNamesDialog.SelectFiles")
        };
        var dialog = new SelectableNamesDialog(vm, _contentDialogService);

        var dialogResult = await dialog.ShowAsync();
        if(dialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary) {
            SelectedFiles = new ObservableCollection<FileViewModel>(vm.GetSelectedEntities<FileViewModel>());
        }
    }

    private bool CanSelectFiles() {
        return AllFiles is not null && SelectedFiles is not null;
    }

    private async Task SelectProvidersAsync() {
        var vm = new SelectableNamesViewModel(AllProviders, SelectedProviders) {
            Title = _localizationService.GetLocalizedString("SelectableNamesDialog.SelectSearchSets")
        };
        var dialog = new SelectableNamesDialog(vm, _contentDialogService);

        var dialogResult = await dialog.ShowAsync();
        if(dialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary) {
            SelectedProviders = new ObservableCollection<FilterProviderViewModel>(
                vm.GetSelectedEntities<FilterProviderViewModel>());
        }
    }


    private bool CanSelectProviders() {
        return AllProviders is not null && SelectedProviders is not null;
    }
}
