using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitAreaBoundaries.ViewModels;

internal class RevitElementGroupViewModel : BaseViewModel {
    private string _name;
    private string _countElements;
    private ObservableCollection<RevitElementViewModel> _revitElementViewModels;

    internal RevitElementGroupViewModel(IEnumerable<RevitElementViewModel> revitElementViewModels, string name) {
        SelectCommand = RelayCommand.Create(SelectAll);
        UnSelectCommand = RelayCommand.Create(UnSelectAll);

        LoadView(revitElementViewModels, name);
    }
    
    public ICommand SelectCommand { get; }
    public ICommand UnSelectCommand { get; }
    
    
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
    public string CountElements {
        get => _countElements;
        set => RaiseAndSetIfChanged(ref _countElements, value);
    }
    
    public ObservableCollection<RevitElementViewModel> RevitElementViewModels {
        get => _revitElementViewModels;
        set => RaiseAndSetIfChanged(ref _revitElementViewModels, value);
    }
    
    // Метод команды на выделение всех уровней
    private void SelectAll() {
        foreach(var vm in RevitElementViewModels) {
            vm.IsChecked = true;
        }
    }

    // Метод команды на снятие выделения всех уровней
    private void UnSelectAll() {
        foreach(var vm in RevitElementViewModels) {
            vm.IsChecked = false;
        }
    }
    
    private void LoadView(IEnumerable<RevitElementViewModel> revitElementViewModels, string name) {
        RevitElementViewModels = new ObservableCollection<RevitElementViewModel>(revitElementViewModels);
        foreach(var vm in RevitElementViewModels) {
            vm.PropertyChanged += OnPropertyChanged;
        }
        Name = name;
        UpdateCountElements();
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not RevitElementViewModel vm) {
            return;
        }

        switch(e.PropertyName) {
            case nameof(vm.IsChecked):
                UpdateCountElements();
                break;
        }

    }
    
    private void UpdateCountElements() {
        int allElements = RevitElementViewModels.Count;
        int selectedElements = RevitElementViewModels.Count(vm => vm.IsChecked);
        CountElements = $"{selectedElements}/{allElements}";
    }
}
