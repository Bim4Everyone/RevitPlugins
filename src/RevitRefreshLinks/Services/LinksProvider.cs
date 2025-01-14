using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class LinksProvider : ILinksProvider {
        private readonly IOpenFileDialogService _openFileDialog;

        public LinksProvider(IOpenFileDialogService openFileDialog) {
            _openFileDialog = openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));
        }


        public ICollection<ILink> GetFolderLinks() {
            if(_openFileDialog.ShowDialog()) {
                return _openFileDialog.Files
                    .Select(file => new Link(file.FullName))
                    .ToArray();
            } else {
                throw new OperationCanceledException();
            }
        }

        public ICollection<ILink> GetServerLinks() {
            throw new NotImplementedException();
        }
    }
}
