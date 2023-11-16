namespace RevitMepTotals.Models.Interfaces {
    /// <summary>
    /// Данные из Revit документа по воздуховодам для экспорта
    /// </summary>
    internal interface IDuctData : IMepData {
        /// <summary>
        /// Значение параметра "Размер"
        /// </summary>
        string Size { get; }

        /// <summary>
        /// Длина в мм
        /// </summary>
        double Length { get; }

        /// <summary>
        /// Площадь в м кв.
        /// </summary>
        double Area { get; }
    }
}
