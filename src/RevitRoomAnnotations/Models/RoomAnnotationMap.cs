namespace RevitRoomAnnotations.Models;
public class RoomAnnotationMap {
    public RevitRoom RevitRoom { get; set; }
    public RevitAnnotation RevitAnnotation { get; set; }
    public bool ToDelete { get; set; }
    public bool ToCreate { get; set; }
}

