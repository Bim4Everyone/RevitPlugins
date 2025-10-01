using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.ViewModels.Links;
internal class LinksSelectorViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IDocTypesHandler _bimModelPartsHandler;
    private readonly IDocTypesProvider _docTypesProvider;

    public LinksSelectorViewModel(
        RevitRepository revitRepository,
        IDocTypesHandler bimModelPartsHandler,
        IDocTypesProvider docTypesProvider) {

        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _bimModelPartsHandler = bimModelPartsHandler
            ?? throw new ArgumentNullException(nameof(bimModelPartsHandler));
        _docTypesProvider = docTypesProvider
            ?? throw new ArgumentNullException(nameof(docTypesProvider));

        Links = new ObservableCollection<LinkViewModel>(_revitRepository.GetAllRevitLinkTypes()
            .Where(doc => _docTypesProvider.GetDocTypes().Contains(_bimModelPartsHandler.GetDocType(doc)))
            .Select(link => new LinkViewModel(link))
            .OrderBy(vm => vm.Name));
        SelectedLinks = new ObservableCollection<LinkViewModel>(Links.Where(link => link.IsSelected));
        SelectedLinks.CollectionChanged += OnCollectionChanged;

        ApplyUserSelectionCommand = RelayCommand.Create(ApplyUserSelection);
    }


    public ICommand ApplyUserSelectionCommand { get; }

    public ObservableCollection<LinkViewModel> Links { get; }

    public ObservableCollection<LinkViewModel> SelectedLinks { get; }


    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        UpdateSelection();
    }

    private void UpdateSelection() {
        foreach(var link in Links) {
            link.IsSelected = SelectedLinks.Contains(link);
        }
    }

    private void ApplyUserSelection() {
        _revitRepository.SetRevitLinkTypesToUse(Links
            .Where(vm => vm.IsSelected)
            .Select(vm => vm.GetLinkType()));
    }
}
