using System.Collections.Generic;
using System.Linq;

using RevitRoomAnnotations.Models;


namespace RevitRoomAnnotations.Services;
public class RoomAnnotationMapService : IRoomAnnotationMapService {
    public IEnumerable<RoomAnnotationMap> GetRoomAnnotationMap(
    IEnumerable<RevitRoom> rooms,
    IEnumerable<RevitAnnotation> annotations) {

        static string Key(string id) => id ?? string.Empty;

        var annDict = annotations
         .GroupBy(a => Key(a.CombinedID))
         .ToDictionary(g => g.Key, g => g.First());

        var roomKeys = new HashSet<string>();
        foreach(var room in rooms) {
            string key = Key(room.CombinedId);
            roomKeys.Add(key);

            yield return annDict.TryGetValue(key, out var ann)
                ? new RoomAnnotationMap { RevitRoom = room, RevitAnnotation = ann, ToCreate = false, ToDelete = false }
                : new RoomAnnotationMap { RevitRoom = room, RevitAnnotation = null, ToCreate = true, ToDelete = false };
        }

        foreach(var ann in annotations) {
            string key = Key(ann.CombinedID);
            if(!roomKeys.Contains(key)) {
                yield return new RoomAnnotationMap { RevitRoom = null, RevitAnnotation = ann, ToCreate = false, ToDelete = true };
            }
        }
    }
}
