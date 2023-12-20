using System;

using dosymep.Revit.ServerClient.DataContracts;

namespace RevitToMongoDB.Model {
    public class Param {
        private long _id;
        public long Id { get => _id; set => _id = value; }
        private string _name;
        public string Name { get => _name; set => _name = value; }
        private object _value;
        public object Value { get => _value; set => _value = value; }
        private string _unitType;
        public string UnitType { get => _unitType; set => _unitType = value; }
        private ParamType _paramType;
        public ParamType ParamType { get => _paramType; set => _paramType = value; }
        private string _storageType;
        public string StorageType { get => _storageType; set => _storageType = value; }
        private Guid? _guid;
        public Guid? Guid { get => _guid; set => _guid = value; }
        private string _systemName;
        public string SystemName { get => _systemName; set => _systemName = value; }
    }
}
