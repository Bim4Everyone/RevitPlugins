using System;

namespace RevitRefreshLinks.Services {
    internal class RsOpenFileDialog : IOpenFileDialog {
        public bool AddExtension { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string FileName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string[] FileNames => throw new NotImplementedException();

        public string Filter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string InitialDirectory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool MultiSelect { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool ShowDialog() {
            throw new NotImplementedException();
        }
    }
}
