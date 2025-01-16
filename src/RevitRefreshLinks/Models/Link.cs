using System.IO;

namespace RevitRefreshLinks.Models {
    internal class Link : ILink {
        public Link(string path) {
            if(string.IsNullOrWhiteSpace(path)) {
                throw new System.ArgumentException(nameof(path));
            }
            if(!File.Exists(path)) {
                throw new FileNotFoundException(path);
            }
            FullPath = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(FullPath);
            NameWithExtension = System.IO.Path.GetFileName(FullPath);
        }

        public string FullPath { get; }

        public string NameWithExtension { get; }

        public string Name { get; }
    }
}
