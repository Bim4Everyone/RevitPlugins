using Autodesk.Revit.DB;

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
            switch(parameter.StorageType) {
                case StorageType.Integer:
                    return new ElementParameterValue(parameter.Id, parameter.StorageType, parameter.AsInteger());
                case StorageType.Double:
                    return new ElementParameterValue(parameter.Id, parameter.StorageType, parameter.AsDouble());
                case StorageType.String:
                    return new ElementParameterValue(parameter.Id, parameter.StorageType, parameter.AsString());
                case StorageType.ElementId:
                    return new ElementParameterValue(parameter.Id, parameter.StorageType, parameter.AsElementId());
                default:
                    return null;
            }
        }

        public void TryApply(Parameter parameter) {
            try {
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
            } catch(Autodesk.Revit.Exceptions.ArgumentException) {
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            }
        }
    }
}
