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

namespace RevitClashDetective.Models {
    /// <summary>
    /// ����� ��� ������������� ������������/�������������� ���� <see cref="SystemParam"/> ��� ������������� �������� <see cref="SystemParam.StorageType"/>
    /// </summary>
    internal class RevitParamConverter : JsonConverter {
        private readonly Document _document;
        private readonly SystemParamsConfig _systemParamsConfig = SystemParamsConfig.Instance;
        private readonly SharedParamsConfig _sharedParamsConfig = SharedParamsConfig.Instance;
        private readonly ProjectParamsConfig _projectParamsConfig = ProjectParamsConfig.Instance;

        /// <summary>
        /// ����������� ���������� ���� <see cref="SystemParam"/> ��� ������������� ���������� �������� <see cref="SystemParam.StorageType"/>
        /// </summary>
        /// <param name="document">��������, � ������� �������� �����������</param>
        /// <exception cref="ArgumentNullException">����������, ���� ������� �������� - ������ ������</exception>
        public RevitParamConverter(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            _document = document;
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(RevitParam);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException("��������� ������������ ������ ReadJson �����");
        }

        // �������� reader.Value ���������� ������� �������� ������ JSON
        // ����� reader.Read() ������ ������������� �� 1 ����� � ����� JSON �����
        // https://stackoverflow.com/questions/23017716/json-net-how-to-deserialize-without-using-the-default-constructor
        // https://stackoverflow.com/questions/20995865/deserializing-json-to-abstract-class
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#sample-factory-pattern-converter
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if(reader is null) { throw new ArgumentNullException(nameof(reader)); }


            JObject jobj = JObject.Load(reader);
            string typeName = jobj["$type"].Value<string>()?.Split(',').FirstOrDefault();
            if(string.IsNullOrWhiteSpace(typeName)) {
                throw new JsonSerializationException($"�� ������� �������� �������� ���� ���������");
            }

            if(typeName.Equals(typeof(SystemParam).FullName)) {

                string id = jobj["Id"].Value<string>();
                if(string.IsNullOrWhiteSpace(id)) { throw new JsonSerializationException($"�� ������� �������� �������� {nameof(RevitParam.Id)}"); }
                BuiltInParameter builtInParameter = (BuiltInParameter) Enum.Parse(typeof(BuiltInParameter), id);
                SystemParam systemParam = _systemParamsConfig.CreateRevitParam(_document, builtInParameter);
                return systemParam;

            } else if(typeName.Equals(typeof(SharedParam).FullName)) {

                string name = jobj["Name"].Value<string>();
                if(string.IsNullOrWhiteSpace(name)) { throw new JsonSerializationException($"�� ������� �������� �������� {nameof(RevitParam.Name)}"); }
                try {
                    SharedParam sharedParam = _sharedParamsConfig.CreateRevitParam(_document, name);
                    return sharedParam;
                } catch(ArgumentNullException) {
                    throw new JsonSerializationException($"� ��������� \'{_document.PathName}\' ����������� �������� \'{name}\'");
                }

            } else if(typeName.Equals(typeof(ProjectParam).FullName)) {

                string name = jobj["Name"].Value<string>();
                if(string.IsNullOrWhiteSpace(name)) { throw new JsonSerializationException($"�� ������� �������� �������� {nameof(RevitParam.Name)}"); }
                try {
                    ProjectParam projectParam = _projectParamsConfig.CreateRevitParam(_document, name);
                    return projectParam;
                } catch(ArgumentNullException) {
                    throw new JsonSerializationException($"� ��������� \'{_document.PathName}\' ����������� �������� \'{name}\'");
                }

            } else if(typeName.Equals(typeof(CustomParam).FullName)) {

                return jobj.ToObject<CustomParam>();

            } else {
                throw new JsonSerializationException($"�� �������������� �������� ���� ���������: {typeName}");
            }
        }
    }
}