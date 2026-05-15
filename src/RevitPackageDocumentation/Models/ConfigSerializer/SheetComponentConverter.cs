using System;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

/// <summary>
/// Конвертер для полиморфной десериализации компонентов листа
/// </summary>
public class SheetComponentConverter : JsonConverter {
    private const string _componentTypeProperty = "ComponentType";
    private const string _structuralPlanViewType = "StructuralPlanView";
    private const string _structuralCalloutViewType = "StructuralCalloutView";
    private const string _sectionViewType = "SectionView";
    private const string _scheduleViewType = "ScheduleView";
    private const string _textNoteType = "TextNote";
    private const string _typicalAnnotationType = "TypicalAnnotation";
    private const string _legendViewType = "LegendView";

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(SheetComponentData);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        var jObject = JObject.Load(reader);
        var componentType = jObject[_componentTypeProperty]?.Value<string>();

        if(string.IsNullOrEmpty(componentType))
            throw new JsonSerializationException($"Property '{_componentTypeProperty}' not found in JSON");

        try {
            return componentType switch {
                _structuralPlanViewType => jObject.ToObject<StructuralPlanViewData>(serializer),
                _structuralCalloutViewType => jObject.ToObject<StructuralCalloutViewData>(serializer),
                _sectionViewType => jObject.ToObject<SectionViewData>(serializer),
                _scheduleViewType => jObject.ToObject<ScheduleViewData>(serializer),
                _textNoteType => jObject.ToObject<TextNoteData>(serializer),
                _typicalAnnotationType => jObject.ToObject<TypicalAnnotationData>(serializer),
                _legendViewType => jObject.ToObject<LegendViewData>(serializer),
                _ => throw new NotSupportedException($"Unknown component type: {componentType}")
            };
        } catch(Exception ex) {
            throw new JsonSerializationException(
                $"Error deserializing type '{componentType}'", ex);
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        serializer.Serialize(writer, value);
    }

    public override bool CanWrite => true;
}
