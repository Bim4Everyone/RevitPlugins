using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitFinishingWalls.Exceptions;
using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation.Implements {
    /// <summary>
    /// Создает стену с зависимостью сверху как у помещения
    /// </summary>
    internal class TopConnectedToRoomTopWallCreator : IWallCreator {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public TopConnectedToRoomTopWallCreator(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository
                ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));
        }


        public Wall Create(WallCreationData wallCreationData) {
            Wall wall;
            try {
                wall = Wall.Create(
                    wallCreationData.Document,
                    wallCreationData.Curve,
                    wallCreationData.Room.LevelId,
                    false);
                wall.WallType = (WallType) wallCreationData.Document.GetElement(wallCreationData.WallTypeId);
                wall.SetParamValue(BuiltInParameter.WALL_BASE_OFFSET, wallCreationData.BaseOffset);
                wall.SetParamValue(BuiltInParameter.WALL_HEIGHT_TYPE, wallCreationData.Room.UpperLimit.Id);
                wall.SetParamValue(BuiltInParameter.WALL_TOP_OFFSET, wallCreationData.Room.LimitOffset);

            } catch(Autodesk.Revit.Exceptions.ArgumentOutOfRangeException) {
                throw new CannotCreateWallException(
                    _localizationService.GetLocalizedString("Exceptions.InvalidHeight"));
            } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                throw new CannotCreateWallException(
                    _localizationService.GetLocalizedString("Exceptions.InvalidLine"));
            }
            _revitRepository.SetWallAxis(wall);
            _revitRepository.SetWallRoomBounding(wall);

            return wall;
        }
    }
}
