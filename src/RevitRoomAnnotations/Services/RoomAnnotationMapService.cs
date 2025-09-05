using System.Collections.Generic;
using System.Linq;

using RevitRoomAnnotations.Models;

namespace RevitRoomAnnotations.Services;
public class RoomAnnotationMapService : IRoomAnnotationMapService {
    public IEnumerable<RoomAnnotationMap> GetRoomAnnotationMap(
    IEnumerable<RevitRoom> rooms,
    IEnumerable<RevitAnnotation> annotations) {

        string Key(int roomId, int linkInstId) => $"{roomId}_{linkInstId}";

        var annDict = annotations.ToDictionary(
            a => Key(a.RoomIdInAnnotation, a.LinkInstIdInAnnotation),
            a => a);

        var roomKeys = new HashSet<string>();
        foreach(var room in rooms) {
            var key = Key(room.RoomId.IntegerValue, room.SourceLinkInstanceId.IntegerValue);
            roomKeys.Add(key);

            if(annDict.TryGetValue(key, out var ann)) {
                yield return new RoomAnnotationMap { RevitRoom = room, RevitAnnotation = ann, ToCreate = false, ToDelete = false };
            } else {
                yield return new RoomAnnotationMap { RevitRoom = room, RevitAnnotation = null, ToCreate = true, ToDelete = false };
            }
        }

        foreach(var ann in annotations) {
            var key = Key(ann.RoomIdInAnnotation, ann.LinkInstIdInAnnotation);
            if(!roomKeys.Contains(key)) {
                yield return new RoomAnnotationMap { RevitRoom = null, RevitAnnotation = ann, ToCreate = false, ToDelete = true };
            }
        }
    }

}
