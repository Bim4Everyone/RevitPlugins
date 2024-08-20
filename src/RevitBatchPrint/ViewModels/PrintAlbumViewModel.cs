using dosymep.WPF.ViewModels;

namespace RevitBatchPrint.ViewModels {
    internal class PrintAlbumViewModel : SelectableObjectViewModel<string> {
        public PrintAlbumViewModel(string objectData) 
            : base(objectData) {
            Name = objectData;
        }

        public PrintAlbumViewModel(string objectData, bool isSelected) 
            : base(objectData, isSelected) {
            Name = objectData;
        }

        public int Count { get; set; }
        public string Name { get; set; }

        public override string DisplayData => this.ToString();

        public override string ToString() {
            return $"{Name} [{Count}]";
        }
    }
}
