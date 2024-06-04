using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitFinishingWalls.Exceptions;
using RevitFinishingWalls.Models;
using RevitFinishingWalls.Services.FailureHandlers;
using RevitFinishingWalls.ViewModels;

namespace RevitFinishingWalls.Services.Creation.Implements {
    internal class RoomFinisher : IRoomFinisher {
        private readonly RevitRepository _revitRepository;
        private readonly IWallCreationDataProvider _wallCreationDataProvider;

        public RoomFinisher(
            RevitRepository revitRepository,
            IWallCreationDataProvider wallCreationDataProvider
            ) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _wallCreationDataProvider = wallCreationDataProvider
                ?? throw new ArgumentNullException(nameof(wallCreationDataProvider));
        }


        public ICollection<RoomErrorsViewModel> CreateWallsFinishing(PluginConfig config) {
            if(config is null) { throw new ArgumentNullException(nameof(config)); }

            var rooms = _revitRepository.GetRooms(config.RoomGetterMode);
            List<RoomErrorsViewModel> errors = new List<RoomErrorsViewModel>();

            using(var transaction = _revitRepository.Document.StartTransaction("Создание отделочных стен")) {
                FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new WallAndRoomSeparationLineOverlapHandler());
                transaction.SetFailureHandlingOptions(failOpt);
                foreach(var room in rooms) {
                    IList<WallCreationData> datas = _wallCreationDataProvider.GetWallCreationData(room, config);
                    RoomErrorsViewModel roomErrors = new RoomErrorsViewModel(room);
                    for(int i = 0; i < datas.Count; i++) {
                        try {
                            var wall = _revitRepository.CreateWall(
                                datas[i],
                                out ICollection<ElementId> notJoinedElements);
                            if(notJoinedElements.Count > 0) {
                                roomErrors.Errors.Add(
                                    new ErrorViewModel(
                                        "Ошибки соединения стен",
                                        "Не удалось присоединить отделочную стену",
                                        new HashSet<ElementId>(notJoinedElements.Union(new ElementId[] { wall.Id }))));
                            }
                        } catch(CannotCreateWallException e) {
                            roomErrors.Errors.Add(new ErrorViewModel("Ошибки создания стен", e.Message, room.Id));
                        }
                    }
                    if(roomErrors.Errors.Count > 0) {
                        errors.Add(roomErrors);
                    }
                }
                transaction.Commit();
            }
            return errors;
        }
    }
}
