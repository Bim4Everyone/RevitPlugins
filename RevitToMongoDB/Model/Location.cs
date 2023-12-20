namespace RevitToMongoDB.Model {
    public class Location {
        private XYZ _max;
        public XYZ Max { get => _max; set => _max = value; }
        private XYZ _min;
        public XYZ Min { get => _min; set => _min = value; }
        private XYZ _mid;
        public XYZ Mid { get => _mid; set => _mid = value; }
    }
}
