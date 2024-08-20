using System.Collections.Generic;

using Autodesk.Revit.DB.Architecture;

using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation {
    internal interface IWallCreationDataProvider {
        /// <summary>
        /// Возвращает список классов с данными для создания отделочных стен
        /// </summary>
        /// <param name="room">Помещение, в котором нужно создать отделочные стены</param>
        /// <param name="settings">Настройки создания отделочных стен</param>
        IList<WallCreationData> GetWallCreationData(Room room, RevitSettings settings);
    }
}
