namespace RevitMepTotals.Models.Interfaces {
    internal interface IPipeInsulationData : IMepData {
        /// <summary>
        /// Значение параметра "Размер трубы"
        /// </summary>
        string PipeSize { get; }

        /// <summary>
        /// Значение параметра "Толщина"
        /// </summary>
        double Thickness { get; }

        /// <summary>
        /// Длина в мм
        /// </summary>
        double Length { get; }
    }
}
