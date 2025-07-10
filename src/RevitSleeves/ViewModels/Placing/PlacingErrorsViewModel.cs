using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitSleeves.Models;
using RevitSleeves.Services.Placing;

namespace RevitSleeves.ViewModels.Placing;
internal class PlacingErrorsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IPlacingErrorsService _placingErrorsService;

    public PlacingErrorsViewModel(
        RevitRepository revitRepository,
        IPlacingErrorsService placingErrorsService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _placingErrorsService = placingErrorsService ?? throw new ArgumentNullException(nameof(placingErrorsService));

        Errors = [.. _placingErrorsService.GetAllErrors().Select(e => new ErrorViewModel(e))];
        ShowDependentElementsCommand = RelayCommand.Create<ErrorViewModel>(
            ShowDependentElements, CanShowDependentElements);
    }


    public ICommand ShowDependentElementsCommand { get; }

    public ObservableCollection<ErrorViewModel> Errors { get; }


    private void ShowDependentElements(ErrorViewModel error) {
        var clashRepo = _revitRepository.GetClashRevitRepository();
        ElementModel[] elementModels = [.. error.ErrorModel.GetDependentElements().Select(e => new ElementModel(e))];
        foreach(var elementModel in elementModels) {
            var transform = elementModel.GetDocInfo(clashRepo.DocInfos)?.Transform ?? Transform.Identity;
            elementModel.TransformModel = new TransformModel(transform);
        }
        clashRepo.SelectAndShowElement(elementModels);
    }

    private bool CanShowDependentElements(ErrorViewModel error) {
        return error is not null;
    }
}
