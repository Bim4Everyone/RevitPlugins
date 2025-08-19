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

    /// <summary>
    /// Базовый конструктор, после вызова конструктора необходимо вызвать метод <see cref="Initialize"/>
    /// </summary>
    public SearchSetViewModel(RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        Filter = filter ?? throw new System.ArgumentNullException(nameof(filter));
        FilterGenerator = generator ?? throw new System.ArgumentNullException(nameof(generator));
        Elements = [];
        ShowElementCommand = RelayCommand.Create<ElementViewModel>(ShowElement, CanShowElement);
    }


    public ICommand ShowElementCommand { get; }

    public RevitFilterGenerator FilterGenerator { get; }

    public Filter Filter { get; }

    public ObservableCollection<ElementViewModel> Elements { get; }


    public void Initialize() {
        Elements.Clear();
        foreach(var element in GetElements()) {
            Elements.Add(element);
        }
    }

    protected abstract ICollection<ElementViewModel> GetElements();

    private void ShowElement(ElementViewModel element) {
        _revitRepository.GetClashRevitRepository().SelectAndShowElement([element.ElementModel]);
    }

    private bool CanShowElement(ElementViewModel element) {
        return element is not null;
    }
}
