using System.Collections.Generic;

namespace RevitMepTotals.Models.Interfaces {
    /// <summary>
    /// Данные документа Revit для экспорта
    /// </summary>
    internal interface IDocumentData {
        /// <summary>
        /// Название документа Revit
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Данные по воздуховодам
        /// </summary>
        ICollection<IDuctData> Ducts { get; }

        /// <summary>
        /// Данные по трубам
        /// </summary>
        ICollection<IPipeData> Pipes { get; }

        /// <summary>
        /// Данные по изоляции трубопроводов
        /// </summary>
        ICollection<IPipeInsulationData> PipeInsulations { get; }
    }
}
