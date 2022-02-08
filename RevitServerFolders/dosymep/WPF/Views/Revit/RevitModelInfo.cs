using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

namespace dosymep.WPF.Views.Revit {
    internal class RevitModelInfo : RevitServerInfo {
        public RevitModelInfo(IRevitServerClient revitServerClient)
            : base(revitServerClient) {
        }

        public RevitModel RevitModel {
            get { return (RevitModel) RevitResponse; }
        }

        public override string Name => RevitModel?.Name; 
        public override string Path => System.IO.Path.Combine(RevitContents?.Path, Name);
        public override string ImageSource { get; } = "Resources/revit.png";

        protected override Task<ObservableCollection<RevitServerInfo>> GetChildren(CancellationToken cancellationToken = default) {
            return Task.FromResult(new ObservableCollection<RevitServerInfo>());
        }
    }
}