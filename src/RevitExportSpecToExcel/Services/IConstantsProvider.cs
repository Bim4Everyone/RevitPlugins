using System.Collections.Generic;

namespace RevitExportSpecToExcel.Services;

internal interface IConstantsProvider {
    /// <summary>
    /// Расширение создаваемого файла Excel
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Максимально допустимая длина названия документа
    /// </summary>
    int DocNameMaxLength { get; }

    /// <summary>
    /// Символы, которые нельзя использовать в названиях листов Excel
    /// </summary>
    IReadOnlyCollection<char> ProhibitedExcelChars { get; }
}

