using System;

using dosymep.SimpleServices;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

/// <summary>
/// Конвертер для полиморфной десериализации параметров плагина
/// </summary>
public class PluginParamConverter : JsonConverter {
    private readonly ILocalizationService _localizationService;

    private const string _pluginParamTypeProperty = "PluginParamType";
    private const string _stringParamType = "StringParam";
    private const string _selectElemParamType = "SelectElem";

    public PluginParamConverter(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(PluginParamData);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        var jObject = JObject.Load(reader);
        var paramType = jObject[_pluginParamTypeProperty]?.Value<string>();

        if(string.IsNullOrEmpty(paramType))
            throw new JsonSerializationException(
                $"{_localizationService.GetLocalizedString("MainViewModel.Property")} '{_pluginParamTypeProperty}' " +
                $"{_localizationService.GetLocalizedString("MainViewModel.NotFoundInJSON")}");

        try {
            return paramType switch {
                _stringParamType => jObject.ToObject<StringParamData>(serializer),
                _selectElemParamType => jObject.ToObject<SelectElemParamData>(serializer),
                _ => throw new NotSupportedException(
                    $"{_localizationService.GetLocalizedString("MainViewModel.UnknownPluginParamType")}: {paramType}")
            };
        } catch(Exception ex) {
            throw new JsonSerializationException(
                $"{_localizationService.GetLocalizedString("MainViewModel.ErrorDeserializingType")} '{paramType}'", ex);
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        serializer.Serialize(writer, value);
    }

    public override bool CanWrite => true;
}
