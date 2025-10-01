using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.CustomParams;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitClashDetective.Models;
/// <summary>
/// Класс для корректировки сериализации/десериализации типа <see cref="SystemParam"/> для корректировки свойства <see cref="SystemParam.StorageType"/>
/// </summary>
internal class RevitParamConverter : JsonConverter {
    private readonly Document _document;
    private readonly SystemParamsConfig _systemParamsConfig = SystemParamsConfig.Instance;
    private readonly SharedParamsConfig _sharedParamsConfig = SharedParamsConfig.Instance;
    private readonly ProjectParamsConfig _projectParamsConfig = ProjectParamsConfig.Instance;

    /// <summary>
    /// Конструктор конвертера типа <see cref="SystemParam"/> для корректировки назначения свойства <see cref="SystemParam.StorageType"/>
    /// </summary>
    /// <param name="document">Документ, в котором запущена конвертация</param>
    /// <exception cref="ArgumentNullException">Исключение, если входной параметр - пустая ссылка</exception>
    public RevitParamConverter(Document document) {
        if(document is null) { throw new ArgumentNullException(nameof(document)); }

        _document = document;
    }

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(RevitParam);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        throw new NotImplementedException("Конвертер поддерживает только ReadJson метод");
    }

    // свойство reader.Value возвращает текущее значение токена JSON
    // метод reader.Read() читает передвигается на 1 токен к концу JSON файла
    // https://stackoverflow.com/questions/23017716/json-net-how-to-deserialize-without-using-the-default-constructor
    // https://stackoverflow.com/questions/20995865/deserializing-json-to-abstract-class
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#sample-factory-pattern-converter
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        if(reader is null) { throw new ArgumentNullException(nameof(reader)); }

        var jobj = JObject.Load(reader);
        string typeName = jobj["$type"].Value<string>()?.Split(',').FirstOrDefault();
        if(string.IsNullOrWhiteSpace(typeName)) {
            throw new JsonSerializationException($"Не удалось получить название типа параметра");
        }

        if(typeName.Equals(typeof(SystemParam).FullName)) {

            string id = jobj["Id"].Value<string>();
            if(string.IsNullOrWhiteSpace(id)) { throw new JsonSerializationException($"Не удалось получить свойство {nameof(RevitParam.Id)}"); }
            var builtInParameter = (BuiltInParameter) Enum.Parse(typeof(BuiltInParameter), id);
            var systemParam = _systemParamsConfig.CreateRevitParam(_document, builtInParameter);
            return systemParam;

        } else if(typeName.Equals(typeof(SharedParam).FullName)) {

            string name = jobj["Name"].Value<string>();
            if(string.IsNullOrWhiteSpace(name)) { throw new JsonSerializationException($"Не удалось получить свойство {nameof(RevitParam.Name)}"); }
            try {
                return _sharedParamsConfig.CreateRevitParam(_document, name);
            } catch(ArgumentNullException) {
                throw new JsonSerializationException($"В документе \'{_document.PathName}\' отсутствует общий параметр \'{name}\'");
            }

        } else if(typeName.Equals(typeof(ProjectParam).FullName)) {

            string name = jobj["Name"].Value<string>();
            if(string.IsNullOrWhiteSpace(name)) { throw new JsonSerializationException($"Не удалось получить свойство {nameof(RevitParam.Name)}"); }
            try {
                return _projectParamsConfig.CreateRevitParam(_document, name);
            } catch(ArgumentNullException) {
                throw new JsonSerializationException($"В документе \'{_document.PathName}\' отсутствует параметр проекта \'{name}\'");
            }

        } else {
            return typeName.Equals(typeof(CustomParam).FullName)
                ? (object) jobj.ToObject<CustomParam>()
                : throw new JsonSerializationException($"Не поддерживаемое название типа параметра: {typeName}");

        }
    }
}
