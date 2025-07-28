using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitMepTotals.Services.Implements;
internal class ConstantsProvider : IConstantsProvider {


    /// <summary>
    /// При экспорте для каждого документа создается лист в Excel, имя которого соответствует заголовку документа.
    /// Правила именования листов Excel:
    /// https://docs.devexpress.com/OfficeFileAPI/DevExpress.Spreadsheet.Worksheet.Name#remarks
    /// </summary>
    public int DocNameMaxLength => 31;

    /// <summary>
    /// Символы, которые нельзя использовать в названии документа:
    /// '\\', '/', '?', ':', '*', '[', ']', '\''
    /// </summary>
    public IReadOnlyCollection<char> ProhibitedExcelChars { get; } = new ReadOnlyCollection<char>(new char[] { '\\', '/', '?', ':', '*', '[', ']', '\'' });
}
