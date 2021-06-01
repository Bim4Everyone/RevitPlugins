using System.Collections.Generic;

namespace dosymep.Revit.ServerClient {
    public class RevitDirectory {
        public string Path { get; set; }
       
        public int LockState { get; set; }
        public string LockContext { get; set; }

        public List<RevitFile> Models { get; set; }
        public List<RevitFolder> Folders { get; set; }

        public override string ToString() {
            return Path;
        }
    }
}
