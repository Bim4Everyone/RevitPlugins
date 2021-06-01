namespace dosymep.Revit.ServerClient {
    public abstract class RevitFileSystem {
        public string Name { get; set; }
        public RevitDirectory Directory { get; set; }

        public int LockState { get; set; }
        public string LockContext { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
