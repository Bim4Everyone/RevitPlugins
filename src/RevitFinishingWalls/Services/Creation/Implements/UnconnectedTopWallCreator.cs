using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitFinishingWalls.Exceptions;
using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation.Implements {
    /// <summary>
    /// Создает стену с неприсоединенной высотой, с заданной отметкой верха от уровня
    /// </summary>
    internal class UnconnectedTopWallCreator : IWallCreator {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public UnconnectedTopWallCreator(RevitRepository revitRepository, ILocalizationService localizationService) {
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
                    wallCreationData.WallTypeId,
                    wallCreationData.Room.LevelId,
                    wallCreationData.TopElevation - wallCreationData.BaseOffset,
                    wallCreationData.BaseOffset,
                    false,
                    false);
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
