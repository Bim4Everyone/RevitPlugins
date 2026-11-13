using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Utils;

internal class SelectableNamesViewModel : BaseViewModel {
    private string _title;
    private bool _allSelected;

    public SelectableNamesViewModel(string[] allNames, string[] selectedNames) {
        if(allNames is null) {
            throw new ArgumentNullException(nameof(allNames));
        }

        if(selectedNames is null) {
            throw new ArgumentNullException(nameof(selectedNames));
        }

        Entities = new ObservableCollection<SelectableNameViewModel>(
            allNames.Distinct().Select(e => new SelectableNameViewModel(e)));
        foreach(var entity in Entities) {
            if(selectedNames.Any(s => entity.Name.Equals(s, StringComparison.CurrentCultureIgnoreCase))) {
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

    public ICollection<string> GetSelectedEntities() {
        return [.. Entities.Where(e => e.IsSelected).Select(e => e.Name)];
    }
}
