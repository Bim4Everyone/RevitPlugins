using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.ClashDetective;
internal class SelectableNamesViewModel : BaseViewModel {
    private string _title;
    private bool _allSelected;

    public SelectableNamesViewModel(IEnumerable<IName> allEntities, IEnumerable<IName> currentSelection) {
        if(allEntities is null) {
            throw new ArgumentNullException(nameof(allEntities));
        }

        if(currentSelection is null) {
            throw new ArgumentNullException(nameof(currentSelection));
        }

        Entities = new ObservableCollection<SelectableNameViewModel>(
            allEntities.Select(e => new SelectableNameViewModel(e)));
        foreach(var entity in Entities) {
            if(currentSelection.Any(s => entity.Name.Equals(s.Name, StringComparison.CurrentCultureIgnoreCase))) {
                entity.IsSelected = true;
            }
        }
    }

    public string Title {
        get => _title;
        set => RaiseAndSetIfChanged(ref _title, value);
    }

    public bool AllSelected {
        get => _allSelected;
        set {
            if(_allSelected != value) {
                RaiseAndSetIfChanged(ref _allSelected, value);
                foreach(var entity in Entities) {
                    entity.IsSelected = value;
                }
            }
        }
    }

    public ObservableCollection<SelectableNameViewModel> Entities { get; }

    public ICollection<T> GetSelectedEntities<T>() where T : IName {
        return [.. Entities.Where(e => e.IsSelected).Select(e => e.Entity).OfType<T>()];
    }
}
