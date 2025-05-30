using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitFinishingWalls.Exceptions;
using RevitFinishingWalls.Models;
using RevitFinishingWalls.Services.FailureHandlers;
using RevitFinishingWalls.ViewModels;

namespace RevitFinishingWalls.Services.Creation.Implements {
    internal class RoomFinisher : IRoomFinisher {
        private readonly RevitRepository _revitRepository;
        private readonly IWallCreationDataProvider _wallCreationDataProvider;
        private readonly ILocalizationService _localizationService;

        public RoomFinisher(
            RevitRepository revitRepository,
            IWallCreationDataProvider wallCreationDataProvider,
            ILocalizationService localizationService
            ) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _wallCreationDataProvider = wallCreationDataProvider
                ?? throw new ArgumentNullException(nameof(wallCreationDataProvider));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }


        public ICollection<RoomErrorsViewModel> CreateWallsFinishing(
            ICollection<Room> rooms,
            RevitSettings settings,
            IWallCreator wallCreator,
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            if(rooms is null) { throw new ArgumentNullException(nameof(rooms)); }
            if(settings is null) { throw new ArgumentNullException(nameof(settings)); }
            if(wallCreator is null) { throw new ArgumentNullException(nameof(wallCreator)); }

            List<RoomErrorsViewModel> errors = new List<RoomErrorsViewModel>();

            using(var transaction = _revitRepository.Document.StartTransaction(
                _localizationService.GetLocalizedString("Transactions.CreateFinishingWalls"))) {
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
                            new ErrorViewModel(
                                _localizationService.GetLocalizedString("ErrorsWindow.ErrorTitles.RoomBoundaries"),
                                ex.Message,
                                room.Id));
                        errors.Add(roomErrors);
                        continue;
                    }
                    for(int i = 0; i < datas.Count; i++) {
                        try {
                            var wall = wallCreator.Create(datas[i]);
                            var notJoinedElements = _revitRepository.JoinElementsToWall(wall, datas[i].ElementsForJoin);
                            if(notJoinedElements.Count > 0) {
                                roomErrors.Errors.Add(
                                    new ErrorViewModel(
                                        _localizationService.GetLocalizedString("ErrorsWindow.ErrorTitles.WallJoining"),
                                        _localizationService.GetLocalizedString("ErrorsWindow.ErrorMsg.WallJoining"),
                                        [.. notJoinedElements, wall.Id]));
                            }
                        } catch(CannotCreateWallException e) {
                            roomErrors.Errors.Add(new ErrorViewModel(
                                _localizationService.GetLocalizedString("ErrorsWindow.ErrorTitles.WallCreation"),
                                e.Message,
                                room.Id));
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
