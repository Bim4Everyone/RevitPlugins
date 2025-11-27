namespace RevitBuildCoordVolumes.Models.Geometry;
internal class Column {
    public Polygon Polygon { get; set; }
    public double BottomPosition { get; set; }
    public double TopPosition { get; set; }
    public string Floor { get; set; }
}
