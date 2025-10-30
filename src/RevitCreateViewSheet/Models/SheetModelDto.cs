using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal class SheetModelDto {
        public SheetModelDto(SheetModel sheetModel) {
            AlbumBlueprint = sheetModel.AlbumBlueprint;
            SheetCustomNumber = sheetModel.SheetCustomNumber;
            SheetNumber = sheetModel.SheetNumber;
            Name = sheetModel.Name;
            TitleBlockSymbolName = sheetModel.TitleBlockSymbol?.Name;
        }

        [JsonConstructor]
        public SheetModelDto() { }

        public string AlbumBlueprint { get; set; }

        /// <summary>
        /// Ш.Номер листа
        /// </summary>
        public string SheetCustomNumber { get; set; }

        public string SheetNumber { get; set; }

        public string Name { get; set; }

        public string TitleBlockSymbolName { get; set; }

        public SheetModel CreateSheetModel(ICollection<FamilySymbol> titleBlocks, NewEntitySaver entitySaver) {
            if(titleBlocks is null) {
                throw new ArgumentNullException(nameof(titleBlocks));
            }

            if(entitySaver is null) {
                throw new ArgumentNullException(nameof(entitySaver));
            }

            if(titleBlocks.Count < 1) {
                throw new ArgumentOutOfRangeException(nameof(titleBlocks));
            }

            var symbol = titleBlocks.FirstOrDefault(t => t.Name == Name) ?? titleBlocks.First();
            return new SheetModel(symbol, entitySaver) {
                AlbumBlueprint = AlbumBlueprint ?? string.Empty,
                SheetCustomNumber = SheetCustomNumber ?? string.Empty,
                SheetNumber = SheetNumber ?? string.Empty,
                Name = Name ?? string.Empty
            };
        }
    }
}
