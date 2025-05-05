using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

namespace RevitCreateViewSheet.Models {
    internal class SheetsCreationDto {
        [JsonConstructor]
        public SheetsCreationDto() { }

        public SheetsCreationDto(IEnumerable<SheetModel> sheetModels) {
            Sheets = [.. sheetModels.Select(s => new SheetModelDto(s))];
        }

        public List<SheetModelDto> Sheets { get; set; }
    }
}
