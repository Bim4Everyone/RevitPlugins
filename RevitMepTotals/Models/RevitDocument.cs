using System;
using System.Collections.Generic;
using System.IO;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class RevitDocument : IDocument, IEquatable<RevitDocument> {
        private readonly FileInfo _file;

        public RevitDocument(FileInfo file) {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public string Name => _file.Name;

        public string Path => _file.FullName;


        public override bool Equals(object obj) {
            return Equals(obj as RevitDocument);
        }

        public bool Equals(RevitDocument other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return Name == other.Name
                && Path == other.Path;
        }

        public override int GetHashCode() {
            int hashCode = 193482316;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }
    }
}
