using System.Collections.Generic;

using dosymep.SimpleServices;

namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Читает Excel-файл с правилами заполнения типа фасада.
/// Ожидается три столбца:
/// 1 - характеристика функции (со скобками, например "(ФМ)");
/// 2 - сокращение основного материала (например "ОК");
/// 3 - значение для записи в параметр.
/// Первая строка — заголовок, данные начинаются со второй строки.
/// </summary>
internal class FacadeTypeExcelReader : ExcelReaderBase<List<FacadeTypeItem>> {
    public FacadeTypeExcelReader(
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService)
        : base(localizationService, messageBoxService) {
    }

    protected override int FirstDataRow => 2;

    protected override int ColumnCount => 3;

    protected override string EmptySheetMessageKey => "Reader.FacadeTypeFileEmpty";

    protected override List<FacadeTypeItem> BuildResult(object[,] rows) {
        var result = new List<FacadeTypeItem>();

        int rowCount = rows.GetLength(0);

        for(int row = 1; row <= rowCount; row++) {
            string function = GetString(rows[row, 1]);
            string material = GetString(rows[row, 2]);
            string value = GetString(rows[row, 3]);

            if(string.IsNullOrWhiteSpace(function)
               || string.IsNullOrWhiteSpace(material)
               || string.IsNullOrWhiteSpace(value)) {
                continue;
            }

            result.Add(new FacadeTypeItem {
                FunctionCharacteristic = Normalize(function),
                MaterialAbbreviation = Normalize(material),
                Value = value
            });
        }
        return result;
    }

    /// <summary>
    /// Приводит ключевое поле правила (характеристику функции или сокращение материала)
    /// к единому виду для последующего сравнения с распарсенным именем типоразмера стены.
    /// </summary>
    private static string Normalize(string value) {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}
