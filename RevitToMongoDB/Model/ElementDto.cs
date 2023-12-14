using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitToMongoDB.Model {
    public class ElementDto {
        public long Id { get; set; }
        public long TypeId { get; set; }
        public string UniqueId { get; set; }
        public Guid VersionGuid { get; set; }
        public string Name { get; set; }
        public string ModelName { get; set; }
        public Category Category { get; set; }
        public Location Locat {get; set; }
        public List<Param> ParamList { get; set; }
        public string Title { get; set; }

        public ElementDto() {
        }

        public class Location {

            public class XYZ {
                public double X { get; set; }
                public double Y { get; set; }
                public double Z { get; set; }
            }

            public ElementDto.Location.XYZ Max;
            public ElementDto.Location.XYZ Min;
            public ElementDto.Location.XYZ Mid;
        }

        public class Param {
            public long Id { get; set; }
            public string Name { get; set; }
            public object Value { get; set; }
            public string UnitType { get; set; }
            // public ParamType ParamType;
            public StorageType StorageType { get; set; }
            public Guid? Guid { get; set; }
            public string SystemName { get; set; }
            public Param() { }
        }
    }
}
