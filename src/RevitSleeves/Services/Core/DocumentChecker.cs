using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;

using RevitSleeves.Models;
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
                "Exceptions.SleeveFamilyNotFound", NamesProvider.FamilyNameSleeve, NamesProvider.SleeveSymbolName));
        }

        var family = sleeveSymbol.Family;
        var missingParameters = GetMissingParameters(family);
        if(missingParameters.Count > 0) {
            ShowWarning(_localization.GetLocalizedString("Exceptions.SleeveFamilyMissingParameters",
                string.Join("\n", missingParameters)));
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

    private void ShowWarning(string msg) {
        if(_messageBoxService.Show(
            msg,
            _localization.GetLocalizedString("Warning"),
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.No) {
            throw new OperationCanceledException();
        }
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
            NamesProvider.ParameterSleeveIncline,
            NamesProvider.ParameterSleeveThickness,
            NamesProvider.ParameterSleeveDescription,
            NamesProvider.ParameterSleeveEconomic.Name,
            NamesProvider.ParameterSleeveSystem.Name
        ];
        return [.. requiredParameters.Except(existingParameters)];
    }
}
