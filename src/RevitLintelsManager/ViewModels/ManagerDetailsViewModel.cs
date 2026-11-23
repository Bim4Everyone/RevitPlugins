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
        ILocalizationService localization, ManagerItemKey? itemKey = null, ManagerGroupKey? groupKey = null) {
        ItemKey = itemKey;
        GroupKey = groupKey;
        IsOpening = element.IsOpening;
        string family = element.Element?.Information?.FamilyName;
        Subject = string.IsNullOrWhiteSpace(family) ? element.TypeName : $"{family}: {element.TypeName}";
        IsWarnings = warnings;
        IsAnalysisPending = warnings && element.IsAnalysisPending;
        HasWarnings = element.HasWarnings;
        Title = IsAnalysisPending ? element.StatusText : localization.GetLocalizedString(!warnings
            ? "Manager.Information" : HasWarnings ? "Manager.HasWarnings" : "Manager.NoWarnings");
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
                GroupName(info, localization)));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString(
                element.IsOpening ? "Manager.Info.FixedOpening" : "Manager.Info.FixedLintel"),
                info?.IsFixed is bool isFixed
                    ? localization.GetLocalizedString(isFixed ? "Manager.Yes" : "Manager.No")
                    : localization.GetLocalizedString("Manager.Unavailable")));
            fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.Id"),
                info?.DisplayId ?? info?.ElementId?.ToString(CultureInfo.InvariantCulture)
                    ?? localization.GetLocalizedString("Manager.Unavailable")));
            if(element.IsAnalysisPending && element.Element is not null) {
                AddProjectFields(fields, element, localization);
            }
        }
        Fields = fields.AsReadOnly();
        ElementWarnings = warnings ? Format(element.Element?.Diagnostics, localization) : Array.Empty<string>();
        RowWarnings = warnings ? Format(element.RowDiagnostics, localization) : Array.Empty<string>();
        var descriptions = ElementWarnings.Concat(RowWarnings).Distinct().ToList();
        if(warnings && HasWarnings && descriptions.Count == 0) {
            descriptions.Add(element.StatusText);
        }
        Descriptions = descriptions.AsReadOnly();
        PositiveMessage = IsStatusOk ? localization.GetLocalizedString(element.IsOpening
            ? "Manager.CorrectOpening" : "Manager.CorrectLintel") : null;
        EmptyWarningsText = IsStatusOk ? localization.GetLocalizedString("Manager.NoWarnings") : null;
    }

    public string Title { get; }
    public ManagerItemKey? ItemKey { get; }
    public ManagerGroupKey? GroupKey { get; }
    public bool IsOpening { get; }
    public string Subject { get; }
    public bool IsWarnings { get; }
    public bool IsAnalysisPending { get; }
    public bool HasWarnings { get; }
    public bool IsStatusOk => IsWarnings && !HasWarnings && !IsAnalysisPending;
    public string PositiveMessage { get; }
    public string Status { get; }
    public IReadOnlyList<ManagerDetailField> Fields { get; }
    public IReadOnlyList<string> ElementWarnings { get; }
    public IReadOnlyList<string> RowWarnings { get; }
    public IReadOnlyList<string> Descriptions { get; }
    public bool HasDescriptions => Descriptions.Count > 0;
    public bool HasRowWarnings => RowWarnings.Count > 0;
    public string EmptyWarningsText { get; }

    // Показывает измеренные значения параметров и стадии в существующем списке сведений.
    private static void AddProjectFields(ICollection<ManagerDetailField> fields, ManagerElementViewModel element,
        ILocalizationService localization) {
        var presentation = element.Element;
        var phases = presentation.Information?.Phases;
        fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.OpeningWidth"),
            Length(presentation.WidthMm, localization)));
        fields.Add(new ManagerDetailField(localization.GetLocalizedString(element.IsOpening
                ? "Manager.Info.OpeningHeight" : "Manager.Info.WallThickness"),
            Length(element.IsOpening ? presentation.HeightMm : presentation.ThicknessMm, localization)));
        fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.CreatedPhase"),
            DisplayPhase(phases?.CreatedName, localization)));
        fields.Add(new ManagerDetailField(localization.GetLocalizedString("Manager.Info.DemolishedPhase"),
            DisplayPhase(phases?.DemolishedName, localization)));
    }

    // Сохраняет нулевое значение длины и показывает миллиметры в текущей культуре.
    private static string Length(double? value, ILocalizationService localization) {
        return value.HasValue ? string.Format(CultureInfo.CurrentCulture,
            localization.GetLocalizedString("Manager.Info.Millimeters"), value.Value)
            : localization.GetLocalizedString("Manager.Unavailable");
    }

    // Отличает явно неназначенную стадию от недоступной.
    private static string DisplayPhase(string name, ILocalizationService localization) {
        return name == string.Empty ? localization.GetLocalizedString("Manager.No") : Display(name, localization);
    }

    // Показывает принадлежность к группе даже при недоступном имени группы.
    private static string GroupName(ManagerElementInfo info, ILocalizationService localization) {
        if(info?.IsInGroup == false || info?.GroupName == string.Empty) {
            return localization.GetLocalizedString("Manager.No");
        }
        return info?.IsInGroup == true && string.IsNullOrWhiteSpace(info.GroupName)
            ? localization.GetLocalizedString("Manager.Yes") : Display(info?.GroupName, localization);
    }

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
