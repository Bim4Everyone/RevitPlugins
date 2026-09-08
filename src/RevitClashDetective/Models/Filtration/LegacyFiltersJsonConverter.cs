using System;
using System.Collections.Generic;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Конвертер устаревшего свойства <see cref="FiltersConfig.Filters"/>.
/// <para/>
/// Читает старые поисковые наборы, а если прочитать их не удалось, возвращает пустую коллекцию
/// вместо исключения. Это нужно, чтобы новая версия плагина открывала конфиг,
/// в который она сама записала заглушку <see cref="You_must_update_plugin"/>,
/// а также не падала на конфигах с параметрами, которых нет в текущем документе.
/// Старые версии плагина такого конвертера не имеют и на заглушке падают - это и требуется.
/// </summary>
internal class LegacyFiltersJsonConverter : JsonConverter {
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(List<Filter>);
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer) {
        if(reader is null) {
            throw new ArgumentNullException(nameof(reader));
        }

        if(serializer is null) {
            throw new ArgumentNullException(nameof(serializer));
        }

        if(reader.TokenType == JsonToken.Null) {
            return new List<Filter>();
        }

        var jarray = JArray.Load(reader);
        try {
            return jarray.ToObject<List<Filter>>(serializer) ?? [];
        } catch(JsonException) {
            return new List<Filter>();
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        throw new NotSupportedException();
    }
}
