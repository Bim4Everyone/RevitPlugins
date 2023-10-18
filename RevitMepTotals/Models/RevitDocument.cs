using System;
using System.IO;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    class RevitDocument : IDocument {
        private readonly FileInfo _file;

        public RevitDocument(FileInfo file) {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public string Name => _file.Name;

        public string Path => _file.FullName;
    }
}
