using Autodesk.Revit.DB;

using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation {
    /// <summary>
    /// Создает отделочную стену по входным данным определенным способом
    /// </summary>
    internal interface IWallCreator {
        Wall Create(WallCreationData wallCreationData);
    }
}
