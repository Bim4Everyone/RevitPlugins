namespace RevitRefreshLinks.Models {
    internal class ExplorerSettings : IExplorerSettings {
        public ExplorerSettings() { }

        public string Title { get; set; }

        public string InitialDirectory { get; set; }

        public bool MultiSelect { get; set; }
    }
}
