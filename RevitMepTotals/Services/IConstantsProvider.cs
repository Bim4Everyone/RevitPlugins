using System.Collections.Generic;

namespace RevitMepTotals.Services {
    /// <summary>
    /// Сервис, предоставляющий константы
    /// </summary>
    internal interface IConstantsProvider {
        /// <summary>
        /// Максимально допустимая длина названия документа
        /// </summary>
        int DocNameMaxLength { get; }

        /// <summary>
        /// Символы, которые нельзя использовать в названиях листов Excel
        /// </summary>
        IReadOnlyCollection<char> ProhibitedExcelChars { get; }
    }
}
