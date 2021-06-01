namespace dosymep.Revit.ServerClient {
    public class RevitFile : RevitFileSystem {
        public long ModelSize { get; set; }
        public long SupportSize { get; set; }
        public long ProductVersion { get; set; }
    }
}
