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

    public ErrorsViewModel(RevitRepository revitRepository, IErrorsService errorsService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _ = errorsService ?? throw new ArgumentNullException(nameof(errorsService));

        Errors = [.. errorsService.GetErrors().Select(e => new ErrorViewModel(e))];
        ShowElementsCommand = RelayCommand.Create<ErrorViewModel>(ShowElements, CanShowElements);
    }

    public ICommand ShowElementsCommand { get; }

    public ObservableCollection<ErrorViewModel> Errors { get; }

    private void ShowElements(ErrorViewModel error) {
        var element = error?.Element;
        if(element is null || element.Id == ElementId.InvalidElementId) {
            return;
        }
        var uiDoc = _revitRepository.ActiveUIDocument;
        uiDoc.Selection.SetElementIds(new[] { element.Id });
        uiDoc.ShowElements(new[] { element.Id });
    }

    private bool CanShowElements(ErrorViewModel error) => error is not null;
}
