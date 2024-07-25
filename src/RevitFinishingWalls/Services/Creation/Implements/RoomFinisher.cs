using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

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


        public ICollection<RoomErrorsViewModel> CreateWallsFinishing(
            ICollection<Room> rooms,
            RevitSettings settings,
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            if(rooms is null) { throw new ArgumentNullException(nameof(rooms)); }
            if(settings is null) { throw new ArgumentNullException(nameof(settings)); }
            List<RoomErrorsViewModel> errors = new List<RoomErrorsViewModel>();

            using(var transaction = _revitRepository.Document.StartTransaction("Создание отделочных стен")) {
                FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new WallAndRoomSeparationLineOverlapHandler());
                transaction.SetFailureHandlingOptions(failOpt);
                var iteration = 0;
                foreach(var room in rooms) {
                    ct.ThrowIfCancellationRequested();
                    progress?.Report(iteration++);
                    RoomErrorsViewModel roomErrors = new RoomErrorsViewModel(room);
                    IList<WallCreationData> datas;
                    try {
                        datas = _wallCreationDataProvider.GetWallCreationData(room, settings);
                    } catch(CannotCreateWallException ex) {
                        roomErrors.Errors.Add(
                            new ErrorViewModel("Ошибки обработки контура помещения", ex.Message, room.Id));
                        errors.Add(roomErrors);
                        continue;
                    }
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
