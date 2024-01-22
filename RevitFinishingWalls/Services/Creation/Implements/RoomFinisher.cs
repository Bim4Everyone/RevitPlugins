using System;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation.Implements {
    internal class RoomFinisher : IRoomFinisher {
        private readonly RevitRepository _revitRepository;

        public RoomFinisher(
            RevitRepository revitRepository
            ) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public void CreateWallsFinishing(PluginConfig config, out string error) {
            if(config is null) { throw new ArgumentNullException(nameof(config)); }

            var rooms = _revitRepository.GetRooms(config.RoomGetterMode);
            StringBuilder sb = new StringBuilder();

            using(var transaction = _revitRepository.Document.StartTransaction("BIM: Создание отделочных стен")) {

                var dataTest = new WallCreationData(_revitRepository.Document) {
                    Curve = Line.CreateBound(XYZ.Zero, new XYZ(10, 0, 0)),
                    BaseOffset = 0,
                    Height = 10,
                    LevelId = new ElementId(311),
                    WallTypeId = new ElementId(232827)
                };
                var wall = _revitRepository.CreateWall(dataTest, out _);
                _revitRepository.ActiveUIDocument.Selection.SetElementIds(new ElementId[] { wall.Id });


                //foreach(var room in rooms) {
                //    IList<WallCreationData> datas = _revitRepository.GetWallCreationData(room, config);
                //    for(int i = 0; i < datas.Count; i++) {
                //        try {
                //            var wall = _revitRepository.CreateWall(datas[i], out ICollection<ElementId> notJoinedElements);
                //            if(notJoinedElements.Count > 0) {
                //                sb.AppendLine(
                //                    $"Не удалось соединить стену с Id={wall.Id} и элементы с Id: " +
                //                    $"{string.Join(", ", notJoinedElements)}\n");
                //            }
                //        } catch(CannotCreateWallException e) {
                //            sb.AppendLine(e.Message + "\n");
                //        }
                //    }
                //}
                transaction.Commit();
            }
            error = sb.ToString();
        }
    }
}
