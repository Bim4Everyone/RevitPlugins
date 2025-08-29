using System.Collections.Generic;
using System.Linq;

using RevitRoomAnnotations.Models;

namespace RevitRoomAnnotations.Services;
public class RoomAnnotationMapService : IRoomAnnotationMapService {
    public IEnumerable<RoomAnnotationMap> GetRoomAnnotationMap(
        IEnumerable<RevitRoom> revitRooms,
        IEnumerable<RevitAnnotation> revitAnnotations) {
        var annotationDict = revitAnnotations.ToDictionary(
            x => $"{x.Id}_{x.LinkId}",
            x => x);

        foreach(var room in revitRooms) {
            string key = $"{room.Id}_{room.LinkId}";
            yield return annotationDict.TryGetValue(key, out var ann)
                ? new RoomAnnotationMap {
                    RevitRoom = room,
                    RevitAnnotation = ann,
                    ToDelete = false,
                    ToCreate = false
                }
                : new RoomAnnotationMap {
                    RevitRoom = room,
                    RevitAnnotation = null,
                    ToDelete = false,
                    ToCreate = true
                };
        }

        var roomKeys = new HashSet<string>(revitRooms.Select(r => $"{r.Id}_{r.LinkId}"));
        foreach(var ann in revitAnnotations) {
            string key = $"{ann.Id}_{ann.LinkId}";
            if(!roomKeys.Contains(key)) {
                yield return new RoomAnnotationMap {
                    RevitRoom = null,
                    RevitAnnotation = ann,
                    ToDelete = true,
                    ToCreate = false
                };
            }
        }
    }
}
