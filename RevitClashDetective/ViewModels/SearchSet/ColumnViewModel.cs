using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class ColumnViewModel : BaseViewModel {
        public string FieldName { get; set; }
        public string Header { get; set; }
        public StorageType FieldType { get; set; }
    }
}
