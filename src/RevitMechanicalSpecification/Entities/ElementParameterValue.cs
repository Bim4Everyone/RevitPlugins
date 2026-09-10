using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Entities {
    internal class ElementParameterValue {
        private ElementParameterValue(ElementId parameterId, StorageType storageType, object value) {
            ParameterId = parameterId;
            StorageType = storageType;
            Value = value;
        }

        public ElementId ParameterId { get; }
        public StorageType StorageType { get; }
        public object Value { get; }

        public static ElementParameterValue Create(Parameter parameter) {
            if(parameter.StorageType == StorageType.None) {
                return null;
            }

            return new ElementParameterValue(parameter.Id, parameter.StorageType, parameter.AsObject());
        }

        public void TryApply(Parameter parameter) {
            switch(StorageType) {
                case StorageType.Integer:
                    parameter.Set((int) Value);
                    break;
                case StorageType.Double:
                    parameter.Set((double) Value);
                    break;
                case StorageType.String:
                    if(Value != null) {
                        parameter.Set((string) Value);
                    }
                    break;
                case StorageType.ElementId:
                    parameter.Set((ElementId) Value);
                    break;
            }
        }
    }
}
