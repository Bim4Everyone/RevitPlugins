using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitExportSpecToExcel.Services;

internal class ConstantsProvider : IConstantsProvider {
    public string FileExtension => "xlsx";

    /// <summary>
    /// При экспорте для каждого документа создается лист в Excel, имя которого соответствует заголовку документа.
    /// Правила именования листов Excel:
    /// https://support.microsoft.com/en-us/office/rename-a-worksheet-3f1f7148-ee83-404d-8ef0-9ff99fbad1f9
    /// </summary>
    public int DocNameMaxLength => 31;

    /// <summary>
    /// Символы, которые нельзя использовать в названии документа:
    /// '\\', '/', '?', ':', '*', '[', ']', '\''
    /// </summary>
    public IReadOnlyCollection<char> ProhibitedExcelChars { get; } = 
        new ReadOnlyCollection<char>(new char[] { '\\', '/', '?', ':', '*', '[', ']', '\'' });
}
