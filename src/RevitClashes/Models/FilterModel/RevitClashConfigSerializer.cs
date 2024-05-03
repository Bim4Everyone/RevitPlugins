using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Serialization;

namespace RevitClashDetective.Models.FilterModel {
    internal class RevitClashConfigSerializer : IConfigSerializer {
        private readonly ISerializationBinder _serializationBinder;
        private readonly Document _document;
        private readonly RevitParamConverter _systemParamConverter;
        private readonly ElementIdConverter _elementIdConverter;

        public RevitClashConfigSerializer(ISerializationBinder serializationBinder, Document document) {
            if(serializationBinder is null) { throw new ArgumentNullException(nameof(serializationBinder)); }
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            _serializationBinder = serializationBinder;
            _document = document;
            _systemParamConverter = new RevitParamConverter(_document);
            _elementIdConverter = new ElementIdConverter();
        }


        public T Deserialize<T>(string text) {
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = _serializationBinder,
                Converters = new List<JsonConverter>() { _systemParamConverter, _elementIdConverter }
            });
        }

        public string Serialize<T>(T @object) {
            return JsonConvert.SerializeObject(@object, Formatting.Indented, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = _serializationBinder,
                Converters = new List<JsonConverter>() { _systemParamConverter, _elementIdConverter }
            });
        }
    }
}
