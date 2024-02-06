using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    /// <summary>
    /// Класс, обозначающий колонку в DevExpress Grid
    /// </summary>
    internal class ColumnViewModel : BaseViewModel {
        public string FieldName { get; set; }
        public string Header { get; set; }
        public StorageType FieldType { get; set; }
    }
}
