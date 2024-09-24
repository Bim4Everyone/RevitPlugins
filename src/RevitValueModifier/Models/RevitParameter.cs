using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitParameter {

        public RevitParameter(Parameter parameter) {
            Parameter = parameter;
            Name = Parameter.Definition.Name;
            IsShared = parameter.IsShared;
            IsReadOnly = parameter.IsReadOnly;
        }

        public string Name { get; set; }
        public StorageType StorageType { get; set; }
        public bool IsBuiltin { get; set; }
        public bool IsShared { get; set; }
        public bool IsReadOnly { get; set; }

        public Parameter Parameter { get; set; }

        public override string ToString() => $"{Name} (Общий: {IsShared}; Системный: {IsBuiltin})";
    }
}
