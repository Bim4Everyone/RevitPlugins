using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitToMongoDB.Model {
    public class ElementDto {
        private long _id;
        public long Id { get => _id; set => _id = value; }
        private long _typeId;
        public long TypeId { get => _typeId; set => _typeId = value; }
        private string _uniqueId;
        public string UniqueId { get => _uniqueId; set => _uniqueId = value; }
        private Guid _versionGuid;
        public Guid VersionGuid { get => _versionGuid; set => _versionGuid = value; }
        private string _name;
        public string Name { get => _name; set => _name = value; }
        private string _modelName;
        public string ModelName { get => _modelName; set => _modelName = value; }
        private Category _category;
        public Category Category { get => _category; set => _category = value; }
        public Location Location { get; set; }
        public List<Param> ParamList { get; set; } = new List<Param>();
    }
}
