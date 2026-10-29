using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.RoomDimensions;

/// <summary>
/// Скрывает предупреждения Revit при фиксации транзакции и запоминает их текст для журнала.
/// </summary>
internal class WarningSwallower : IFailuresPreprocessor {
    public List<string> Messages { get; } = [];

    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor) {
        foreach(var message in failuresAccessor.GetFailureMessages()) {
            if(message.GetSeverity() == FailureSeverity.Warning) {
                Messages.Add(message.GetDescriptionText());
                failuresAccessor.DeleteWarning(message);
            }
        }
        return FailureProcessingResult.Continue;
    }
}
