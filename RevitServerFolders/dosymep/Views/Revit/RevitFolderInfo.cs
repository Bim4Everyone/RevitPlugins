using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

namespace dosymep.Views.Revit {
    internal class RevitFolderInfo : RevitServerInfo {
        public RevitFolderInfo(IRevitServerClient revitServerClient)
            : base(revitServerClient) {
        }

        public RevitFolder RevitFolder {
            get { return (RevitFolder) RevitResponse; }
        }

        public override string Name => RevitFolder?.Name;
        public override string Path => System.IO.Path.Combine(RevitContents?.Path, Name);
        public override string ImageSource { get; } = "Resources/folder.png";

        protected async override Task<ObservableCollection<RevitServerInfo>> GetChildren(CancellationToken cancellationToken = default) {
            RevitContents contents = await _revitServerClient.GetContentsAsync(Path, cancellationToken);
            return GetChildren(contents);
        }
    }
}