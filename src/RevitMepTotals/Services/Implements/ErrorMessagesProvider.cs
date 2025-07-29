using System;
using System.Collections.Generic;

using dosymep.SimpleServices;

namespace RevitMepTotals.Services.Implements;
internal class ErrorMessagesProvider : IErrorMessagesProvider {
    private readonly IConstantsProvider _constantsProvider;
    private readonly ILocalizationService _localizationService;

    public ErrorMessagesProvider(IConstantsProvider constantsProvider, ILocalizationService localizationService) {
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public string GetFileNamesConflictMessage(ICollection<string> conflictNames) {
        return _localizationService.GetLocalizedString(
            "Errors.FileNamesConflict",
            string.Join(Environment.NewLine, conflictNames),
            _constantsProvider.DocNameMaxLength,
            string.Join(", ", _constantsProvider.ProhibitedExcelChars));
    }

    public string GetFileAlreadyOpenedMessage(string path) {
        return _localizationService.GetLocalizedString("Errors.CannotProcessOpenedDocument", path);
    }

    public string GetFileVersionIsInvalidMessage(string path) {
        return _localizationService.GetLocalizedString("Errors.NotSupportedDocumentVersion", path);
    }

    public string GetFileDataCorruptionMessage(string path) {
        return _localizationService.GetLocalizedString("Errors.DocumentHasTooManyErrors", path);
    }

    public string GetFileCannotBeProcessMessage(string path) {
        return _localizationService.GetLocalizedString("Errors.CannotProcessDocument", path);
    }

    public string GetFileRemovedMessage(string path) {
        return _localizationService.GetLocalizedString("Errors.DocumentIsRemoved", path);
    }

    public string GetInsufficientResourcesMessage() {
        return _localizationService.GetLocalizedString("Errors.InsufficientResources");
    }
}
