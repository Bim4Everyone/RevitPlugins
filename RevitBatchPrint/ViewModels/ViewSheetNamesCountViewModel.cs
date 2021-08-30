namespace RevitBatchPrint {
    public class ViewSheetNamesCountViewModel {
        public int Count { get; set; }
        public string Name { get; set; }

        public override string ToString() {
            return $"{Name} [{Count}]";
        }
    }
}
