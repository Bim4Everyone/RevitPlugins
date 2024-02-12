namespace RevitMepTotals.Models.Interfaces {
    /// <summary>
    /// Документ Revit для обработки
    /// </summary>
    internal interface IDocument {
        /// <summary>
        /// Название документа
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Путь к Revit документу
        /// </summary>
        string Path { get; }
    }
}
