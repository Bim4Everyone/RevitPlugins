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
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        public string Path { get; }

        public string Name { get; }
    }
}
