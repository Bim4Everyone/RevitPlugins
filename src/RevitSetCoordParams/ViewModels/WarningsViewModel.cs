using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningsViewModel : BaseViewModel {

    private readonly IReadOnlyCollection<WarningModel> _warningModels;

    private ObservableCollection<WarningElementViewModel> _warningElements;

    public WarningsViewModel(IReadOnlyCollection<WarningModel> warningModels) {
        _warningModels = warningModels;

        //LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public ObservableCollection<WarningElementViewModel> WarningElements {
        get => _warningElements;
        set => RaiseAndSetIfChanged(ref _warningElements, value);
    }



    //// Метод загрузки вида
    //private void LoadView() {
    //    WarningElements = new ObservableCollection<WarningElementViewModel>(GetWarningElements());
    //}

    //private IEnumerable<WarningElementViewModel> GetWarningElements() {
    //    var groupedWarnings = _warningModels
    //        .GroupBy(w => w.WarningDescription);
    //}



}
