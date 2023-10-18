namespace RevitMepTotals.Models.Interfaces {
    /// <summary>
    /// Данные из Revit документа по изоляции воздуховодов для экспорта
    /// </summary>
    internal interface IDuctInsulationData : IMepData {
        /// <summary>
        /// Значение параметра "Размер воздуховода"
        /// </summary>
        string DuctSize { get; }

        /// <summary>
        /// Значение параметра "Толщина"
        /// </summary>
        double Thickness { get; }

        /// <summary>
        /// Длина в мм
        /// </summary>
        double Length { get; }

        /// <summary>
        /// Значение параметра "Площадь" в м2
        /// </summary>
        double Area { get; }
    }
}
