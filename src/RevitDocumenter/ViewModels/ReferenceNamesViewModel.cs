using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;

namespace RevitDocumenter.ViewModels;
internal class ReferenceNamesViewModel : BaseViewModel {
    public ReferenceNamesViewModel() {
        VerticalRefNames = [];
        HorizontalRefNames = [];

        AddVerticalNameRefCommand = RelayCommand.Create(AddVerticalNameRef);
        RemoveVerticalNameRefCommand = RelayCommand.Create(RemoveVerticalNameRef, CanRemoveVerticalNameRef);

        AddHorizontalNameRefCommand = RelayCommand.Create(AddHorizontalNameRef);
        RemoveHorizontalNameRefCommand = RelayCommand.Create(RemoveHorizontalNameRef, CanRemoveHorizontalNameRef);
    }

    public ICommand AddVerticalNameRefCommand { get; }
    public ICommand RemoveVerticalNameRefCommand { get; }
    public ICommand AddHorizontalNameRefCommand { get; }
    public ICommand RemoveHorizontalNameRefCommand { get; }

    public ObservableCollection<ReferenceNameViewModel> VerticalRefNames { get; set; }
    public ObservableCollection<ReferenceNameViewModel> HorizontalRefNames { get; set; }

    private void AddVerticalNameRef() => VerticalRefNames.Add(new ReferenceNameViewModel(string.Empty));

    public void RemoveVerticalNameRef() {
        foreach(var item in VerticalRefNames.Where(x => x.IsCheck).ToList()) {
            VerticalRefNames.Remove(item);
        }
    }

    public bool CanRemoveVerticalNameRef() {
        return VerticalRefNames.Any(x => x.IsCheck);
    }

    private void AddHorizontalNameRef() => HorizontalRefNames.Add(new ReferenceNameViewModel(string.Empty));
    public void RemoveHorizontalNameRef() {
        foreach(var item in HorizontalRefNames.Where(x => x.IsCheck).ToList()) {
            HorizontalRefNames.Remove(item);
        }
    }

    public bool CanRemoveHorizontalNameRef() {
        return HorizontalRefNames.Any(x => x.IsCheck);
    }

    public ReferenceNames GetReferenceNames() => new(
        [.. VerticalRefNames.Select(r => r.ReferenceName)],
        [.. HorizontalRefNames.Select(r => r.ReferenceName)]);
}
