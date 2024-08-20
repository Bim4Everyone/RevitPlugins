namespace RevitMepTotals.Models.Interfaces {
    /// <summary>
    /// Данные из Revit документа по трубам для экспорта
    /// </summary>
    internal interface IPipeData : IMepData {
        /// <summary>
        /// Значение параметра "Размер"
        /// </summary>
        string Size { get; }

        /// <summary>
        /// Длина в мм
        /// </summary>
        double Length { get; }
    }
}
