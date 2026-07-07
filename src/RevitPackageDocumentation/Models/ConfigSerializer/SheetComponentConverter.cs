using System;

using dosymep.SimpleServices;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

/// <summary>
/// Конвертер для полиморфной десериализации компонентов листа
/// </summary>
public class SheetComponentConverter : JsonConverter {
    private readonly ILocalizationService _localizationService;

    private const string _componentTypeProperty = "ComponentType";
    private const string _structuralPlanViewType = "PlanView";
    private const string _structuralCalloutViewType = "CalloutView";
    private const string _sectionViewType = "SectionView";
    private const string _scheduleViewType = "ScheduleView";
    private const string _textNoteType = "TextNote";
    private const string _typicalAnnotationType = "TypicalAnnotation";
    private const string _legendViewType = "LegendView";

    public SheetComponentConverter(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(SheetComponentData);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        var jObject = JObject.Load(reader);
        var componentType = jObject[_componentTypeProperty]?.Value<string>();

        if(string.IsNullOrEmpty(componentType))
            throw new JsonSerializationException(
                $"{_localizationService.GetLocalizedString("MainViewModel.Property")} '{_componentTypeProperty}' " +
                $"{_localizationService.GetLocalizedString("MainViewModel.NotFoundInJSON")}");

        try {
            return componentType switch {
                _structuralPlanViewType => jObject.ToObject<PlanViewData>(serializer),
                _structuralCalloutViewType => jObject.ToObject<CalloutViewData>(serializer),
                _sectionViewType => jObject.ToObject<SectionViewData>(serializer),
                _scheduleViewType => jObject.ToObject<ScheduleViewData>(serializer),
                _textNoteType => jObject.ToObject<TextNoteData>(serializer),
                _typicalAnnotationType => jObject.ToObject<TypicalAnnotationData>(serializer),
                _legendViewType => jObject.ToObject<LegendViewData>(serializer),
                _ => throw new NotSupportedException(
                    $"{_localizationService.GetLocalizedString("MainViewModel.UnknownComponentType")}: {componentType}")
            };
        } catch(Exception ex) {
            throw new JsonSerializationException(
                $"{_localizationService.GetLocalizedString("MainViewModel.ErrorDeserializingType")} '{componentType}'", ex);
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        serializer.Serialize(writer, value);
    }

    public override bool CanWrite => true;
}
