using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Services.Core;

namespace RevitSplitMepCurve.ViewModels.Errors;

internal class ErrorsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IErrorsService _errorsService;

    public ErrorsViewModel(RevitRepository revitRepository, IErrorsService errorsService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));

        ShowElementsCommand = RelayCommand.Create<ErrorViewModel>(ShowElements, CanShowElements);
        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand ShowElementsCommand { get; }

    public ICommand LoadViewCommand { get; }

    public ObservableCollection<ErrorViewModel> Errors { get; } = [];

    private void ShowElements(ErrorViewModel error) {
        _revitRepository.ShowElements(error.Element);
    }

    private bool CanShowElements(ErrorViewModel error) => error is not null;

    private void LoadView() {
        Errors.Clear();
        foreach(var error in _errorsService.GetErrors()) {
            Errors.Add(new ErrorViewModel(error));
        }
    }
}
