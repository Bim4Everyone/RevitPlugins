namespace RevitMepTotals.Models.Interfaces {
    /// <summary>
    /// Данные из Revit документа для экспорта
    /// </summary>
    internal interface IMepData {
        /// <summary>
        /// Название типоразмера
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Значение параметра "ФОП_ВИС_Наименование"
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Значение параметра "Имя системы"
        /// </summary>
        string SystemName { get; }
    }
}
