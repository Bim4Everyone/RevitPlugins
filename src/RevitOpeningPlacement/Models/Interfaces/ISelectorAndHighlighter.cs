namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс, предоставляющий экземпляр семейства для выделения на виде и элемент, графику которого надо также выделить
    /// </summary>
    internal interface ISelectorAndHighlighter : IElementsToSelectProvider, IElementToHighlightProvider {
    }
}
