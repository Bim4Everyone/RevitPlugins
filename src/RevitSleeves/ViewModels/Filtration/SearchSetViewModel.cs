using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;

internal abstract class SearchSetViewModel : BaseViewModel {
    protected readonly RevitRepository _revitRepository;

    public SearchSetViewModel(RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        Filter = filter ?? throw new System.ArgumentNullException(nameof(filter));
        FilterGenerator = generator ?? throw new System.ArgumentNullException(nameof(generator));
        Elements = new ObservableCollection<ElementViewModel>(InitializeElements());
        ShowElementCommand = RelayCommand.Create<ElementViewModel>(ShowElement, CanShowElement);
    }


    public ICommand ShowElementCommand { get; }

    public RevitFilterGenerator FilterGenerator { get; }

    public Filter Filter { get; }

    public ObservableCollection<ElementViewModel> Elements { get; }

    protected abstract ICollection<ElementViewModel> InitializeElements();

    private void ShowElement(ElementViewModel element) {
        _revitRepository.GetClashRevitRepository().SelectAndShowElement([element.ElementModel]);
    }

    private bool CanShowElement(ElementViewModel element) {
        return element is not null;
    }
}
