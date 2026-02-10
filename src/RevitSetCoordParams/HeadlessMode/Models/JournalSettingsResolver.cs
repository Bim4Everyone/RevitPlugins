using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;

using RevitSetCoordParams.HeadlessMode.Enums;
using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Settings;

namespace RevitSetCoordParams.HeadlessMode.Models;
internal class JournalSettingsResolver {
    private readonly ConfigSettings _config;
    private readonly ConfigSettings _defaults;
    private readonly IParamAvailabilityService _paramAvailabilityService;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly RevitRepository _revitRepository;

    public JournalSettingsResolver(
        ConfigSettings configSettings,
        ConfigSettings defaultSettings,
        IParamAvailabilityService paramAvailabilityService,
        IRevitParamFactory revitParamFactory,
        RevitRepository revitRepository) {
        _config = configSettings;
        _defaults = defaultSettings;
        _paramAvailabilityService = paramAvailabilityService;
        _revitParamFactory = revitParamFactory;
        _revitRepository = revitRepository;
    }

    public TEnum ResolveEnum<TEnum>(JournalContainer journalContainer, Func<ConfigSettings, TEnum> selector)
    where TEnum : struct, Enum {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(journalContainer, selector, s => Enum.TryParse<TEnum>(s, ignoreCase: true, out var value)
            ? value
            : selector(_defaults));
    }

    public string ResolveString(JournalContainer journalContainer, Func<ConfigSettings, string> selector) {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(journalContainer, selector, s => s);
    }

    public List<string> ResolveListString(JournalContainer journalContainer, Func<ConfigSettings, List<string>> selector) {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(journalContainer, selector, s => {
                string value = journalContainer.JournalValue;
                string[] parts = value?.Split(';');
                return parts?.Length > 1
                    ? parts.Select(x => x.Trim()).ToList()
                    : selector(_defaults);
            });
    }

    public List<TEnum> ResolveListEnum<TEnum>(JournalContainer journalContainer, Func<ConfigSettings, List<TEnum>> selector)
    where TEnum : struct, Enum {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(journalContainer, selector, s => {
                if(string.IsNullOrWhiteSpace(s)) {
                    return selector(_defaults);
                }
                var values = s
                    .Split(';')
                    .Select(x => x.Trim())
                    .Select(x => Enum.TryParse<TEnum>(x, ignoreCase: true, out var parsed)
                        ? (success: true, value: parsed)
                        : (success: false, value: default))
                    .Where(x => x.success)
                    .Select(x => x.value)
                    .Distinct()
                    .ToList();

                return values.Count > 0
                    ? values
                    : selector(_defaults);
            });
    }

    public List<ParamMap> ResolveParamMaps(JournalContainer journalContainer, Func<ConfigSettings, List<ParamMap>> selector, Document document) {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(journalContainer, selector, s => {
                if(string.IsNullOrWhiteSpace(s)) {
                    return selector(_defaults);
                }

                var result = new List<ParamMap>();

                string[] entries = s.Split(';');

                foreach(string entry in entries) {

                    string[] parts = entry.Split(':');
                    if(parts.Length != 2) {
                        continue;
                    }

                    string typePart = parts[0].Trim();
                    string paramsPart = parts[1];

                    if(!Enum.TryParse<ParamType>(typePart, true, out var paramType)) {
                        continue;
                    }

                    string[] paramNames = paramsPart
                        .Split(',')
                        .Select(x => x.Trim())
                        .ToArray();

                    if(paramNames.Length != 2) {
                        continue;
                    }

                    var source = GetParam(document, paramNames[0]);
                    var target = GetParam(document, paramNames[1]);

                    if(source == null && paramType != ParamType.BlockingParam) {
                        var paramMap = paramType switch {
                            ParamType.BlockParam => RevitConstants.BlockParamMap,
                            ParamType.SectionParam => RevitConstants.SectionParamMap,
                            ParamType.FloorParam => RevitConstants.FloorParamMap,
                            ParamType.FloorDEParam => RevitConstants.FloorDEParamMap,
                            _ => null
                        };
                        if(paramMap == null) {
                            continue;
                        }
                        result.Add(paramMap);
                    }

                    if(target == null) {
                        var paramMap = paramType switch {
                            ParamType.BlockParam => RevitConstants.BlockParamMap,
                            ParamType.SectionParam => RevitConstants.SectionParamMap,
                            ParamType.FloorParam => RevitConstants.FloorParamMap,
                            ParamType.FloorDEParam => RevitConstants.FloorDEParamMap,
                            ParamType.BlockingParam => RevitConstants.BlockingParamMap,
                            _ => null
                        };
                        if(paramMap == null) {
                            continue;
                        }
                        result.Add(paramMap);
                    }

                    result.Add(new ParamMap {
                        Type = paramType,
                        SourceParam = source,
                        TargetParam = target
                    });
                }

                return result.Count > 0
                    ? result
                    : selector(_defaults);
            });
    }

    public double ResolveDouble(JournalContainer journalContainer, Func<ConfigSettings, double> selector) {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(
                journalContainer,
                selector,
                s => double.TryParse(
                    s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value)
                        ? value
                        : selector(_defaults));
    }

    public bool ResolveBool(JournalContainer journalContainer, Func<ConfigSettings, bool> selector) {
        return journalContainer == null
            ? selector(_defaults)
            : ResolveValue(
                    journalContainer,
                    selector,
                    s => bool.TryParse(
                        s, out bool value)
                            ? value
                            : selector(_defaults));
    }

    private T ResolveValue<T>(JournalContainer journalContainer, Func<ConfigSettings, T> selector, Func<string, T> parseExplicit) {
        return journalContainer.JournalValue == null
            ? selector(_defaults)
            : journalContainer.SettingType switch {
                SettingType.DefaultSettings => selector(_defaults),
                SettingType.ConfigSettings => selector(_config),
                SettingType.ExplicitSettings => parseExplicit(journalContainer.JournalValue),
                _ => selector(_defaults)
            };
    }

    private RevitParam GetParam(Document document, string paramName) {
        var def = _paramAvailabilityService.GetDefinitionByName(document, paramName);
        return def != null
            ? _revitParamFactory.Create(_revitRepository.Document, def.GetElementId())
            : null;
    }
}
