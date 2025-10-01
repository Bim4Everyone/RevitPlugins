using System.Collections.Generic;

using RevitRoomAnnotations.Models;


namespace RevitRoomAnnotations.Services;
public interface IRoomAnnotationMapService {
    IEnumerable<RoomAnnotationMap> GetRoomAnnotationMap(
        IEnumerable<RevitRoom> rooms,
        IEnumerable<RevitAnnotation> annotations);
}
