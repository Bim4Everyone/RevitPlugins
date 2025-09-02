using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Placing.FamilySymbolFinder;

namespace RevitSleeves.Services.Core;
internal class DocumentChecker : IDocumentChecker {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IFamilySymbolFinder _familySymbolFinder;

    public DocumentChecker(
        RevitRepository revitRepository,
        ILocalizationService localization,
        IMessageBoxService messageBoxService,
        IFamilySymbolFinder familySymbolFinder) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _familySymbolFinder = familySymbolFinder ?? throw new ArgumentNullException(nameof(familySymbolFinder));
    }


    public void CheckDocument() {
        FamilySymbol sleeveSymbol = default;
        try {
            sleeveSymbol = _familySymbolFinder.GetFamilySymbol();
        } catch(InvalidOperationException) {
            ShowError(_localization.GetLocalizedString(
                "Exceptions.SleeveFamilyNotFound",
                NamesProvider.FamilyNameSleeve,
                NamesProvider.SleeveSymbolName,
                Category.GetCategory(_revitRepository.Document, SleevePlacementSettingsConfig.SleeveCategory).Name));
        }

        var family = sleeveSymbol.Family;
        var missingSleeveParameters = GetMissingParameters(family);
        if(missingSleeveParameters.Count > 0) {
            ShowError(_localization.GetLocalizedString("Exceptions.SleeveFamilyMissingParameters",
                string.Join("\n", missingSleeveParameters)));
        }

        var pipeCategory = Category.GetCategory(_revitRepository.Document, BuiltInCategory.OST_PipeCurves);
        var missingPipeParameters = GetMissingParameters(pipeCategory);
        if(missingPipeParameters.Count > 0) {
            ShowError(_localization.GetLocalizedString("Exceptions.CategoryMissingParameters",
                pipeCategory.Name,
                string.Join("\n", missingPipeParameters)));
        }
    }

    private void ShowError(string msg) {
        _messageBoxService.Show(
            msg,
            _localization.GetLocalizedString("Error"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);
        throw new OperationCanceledException();
    }

    private ICollection<string> GetFamilyParameterNames(RevitRepository revitRepository, Family family) {
        using var familyDoc = revitRepository.Document.EditFamily(family);
        string[] paramNames = familyDoc.FamilyManager
            .GetParameters()
            .Select(param => param.Definition.Name)
            .ToArray();
        familyDoc.Close(false);
        return paramNames;
    }

    private ICollection<string> GetMissingParameters(Family family) {
        var existingParameters = GetFamilyParameterNames(_revitRepository, family);
        var paramConfig = SharedParamsConfig.Instance;
        string[] requiredParameters = [
            NamesProvider.ParameterSleeveDiameter,
            NamesProvider.ParameterSleeveLength,
            NamesProvider.ParameterSleeveIncline.Name,
            NamesProvider.ParameterSleeveThickness.Name,
            NamesProvider.ParameterSleeveDescription.Name,
            NamesProvider.ParameterSleeveEconomic.Name,
            NamesProvider.ParameterSleeveSystem.Name
        ];
        return [.. requiredParameters.Except(existingParameters)];
    }

    private ICollection<string> GetMissingParameters(Category category) {
        string[] existingSharedParameters = _revitRepository.Document.GetParameterBindings()
            .Where(item => item.Binding is InstanceBinding)
            .Where(item =>
                ((InstanceBinding) item.Binding).Categories
                .OfType<Category>()
                .Any(c => c.Id == category.Id))
            .Select(item => item.Definition.Name)
            .ToArray();
        var paramConfig = SharedParamsConfig.Instance;
        string[] requiredParameters = [
            NamesProvider.ParameterSleeveEconomic.Name,
            NamesProvider.ParameterSleeveSystem.Name
        ];
        return [.. requiredParameters.Except(existingSharedParameters)];
    }
}
