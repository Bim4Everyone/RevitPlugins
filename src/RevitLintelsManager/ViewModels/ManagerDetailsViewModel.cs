using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Готовое содержимое мини-окна без API-объектов и ссылок на визуальные контейнеры.</summary>
internal sealed class ManagerDetailsViewModel {
    /// <summary>Подготавливает предоставленные сведения или диагностические причины.</summary>
    public ManagerDetailsViewModel(ManagerElementViewModel element, bool warnings,
        ILocalizationService localization) {
        Title = localization.GetLocalizedString(warnings ? "Manager.Warnings" : "Manager.Information");
        Subject = element.TypeName;
        IsWarnings = warnings;
        Status = warnings ? element.StatusText : null;
        var fields = new List<ManagerDetailField>();
        if(!warnings) {
            var info = element.Element?.Information;
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Family"),
                Display(info?.FamilyName, localization)));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Type"), element.TypeName));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Workset"),
                Display(info?.WorksetName, localization)));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Level"),
                Display(info?.LevelName, localization)));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Group"),
                info?.GroupName == string.Empty ? localization.GetLocalizedString("Manager.No")
                    : Display(info?.GroupName, localization)));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString(
                element.IsOpening ? "Manager.Info.FixedOpening" : "Manager.Info.FixedLintel"),
                info?.IsFixed is bool isFixed
                    ? localization.GetLocalizedString(isFixed ? "Manager.Yes" : "Manager.No")
                    : localization.GetLocalizedString("Manager.Unavailable")));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Id"),
                info?.ElementId?.ToString(CultureInfo.InvariantCulture)
                    ?? localization.GetLocalizedString("Manager.Unavailable")));
        }
        Fields = fields.AsReadOnly();
        ElementWarnings = warnings ? Format(element.Element?.Diagnostics, localization) : Array.Empty<string>();
        RowWarnings = warnings ? Format(element.RowDiagnostics, localization) : Array.Empty<string>();
        EmptyWarningsText = warnings && ElementWarnings.Count == 0 && RowWarnings.Count == 0
            ? localization.GetLocalizedString("Manager.NoWarnings") : null;
    }

    public string Title { get; }
    public string Subject { get; }
    public bool IsWarnings { get; }
    public string Status { get; }
    public IReadOnlyList<ManagerDetailField> Fields { get; }
    public IReadOnlyList<string> ElementWarnings { get; }
    public IReadOnlyList<string> RowWarnings { get; }
    public bool HasRowWarnings => RowWarnings.Count > 0;
    public string EmptyWarningsText { get; }

    // Неизвестные данные не заменяются предполагаемыми значениями.
    private static string Display(string value, ILocalizationService localization) {
        return string.IsNullOrWhiteSpace(value) ? localization.GetLocalizedString("Manager.Unavailable") : value;
    }

    // Ошибки будущей обработки используют тот же контракт диагностик.
    private static IReadOnlyList<string> Format(IEnumerable<ManagerDiagnostic> diagnostics,
        ILocalizationService localization) {
        return (diagnostics ?? Array.Empty<ManagerDiagnostic>()).Select(diagnostic => {
            string format = localization.GetLocalizedString(diagnostic.LocalizationKey);
            if(string.IsNullOrWhiteSpace(format)) {
                return localization.GetLocalizedString("Manager.DiagnosticUnavailable");
            }
            try {
                return string.Format(CultureInfo.CurrentCulture, format, diagnostic.Arguments.Cast<object>().ToArray());
            } catch(FormatException) {
                return localization.GetLocalizedString("Manager.DiagnosticUnavailable");
            }
        }).Distinct().ToList().AsReadOnly();
    }
}

/// <summary>Локализованная подпись и готовое значение поля.</summary>
internal sealed class ManagerDetailField(string label, string value) {
    public string Label { get; } = label;
    public string Value { get; } = value;
}
