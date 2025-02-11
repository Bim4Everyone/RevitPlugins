namespace RevitRefreshLinks.Models {
    internal interface IExplorerSettings {
        string Title { get; }
        string InitialDirectory { get; }
        bool MultiSelect { get; }
    }
}
