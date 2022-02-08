using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.Async;
using dosymep.Revit.ServerClient;
using dosymep.WPF.Commands;

namespace dosymep.WPF.Views.Revit {
    public class RevitServerInfo : INotifyPropertyChanged {
        protected readonly IRevitServerClient _revitServerClient;
        private ObservableCollection<RevitServerInfo> _children = new ObservableCollection<RevitServerInfo>() { null };
        private bool _isExpanded;

        public RevitServerInfo(IRevitServerClient revitServerClient) {
            _revitServerClient = revitServerClient;

            Name = _revitServerClient.ServerName;
            ExpandCommand = new RelayCommand(o => {
                if(!ChildrenLoaded) {
                    Children = AsyncHelper.RunSync(() => GetChildren());
                }

                ChildrenLoaded = true;
            });
        }

        public ICommand ExpandCommand { get; }
        public bool ChildrenLoaded { get; private set; }

        public RevitResponse RevitResponse { get; set; }
        public RevitContents RevitContents { get; set; }

        public virtual string Name { get; }
        public virtual string Path { get; }
        public virtual string ImageSource { get; } = "Resources/server.png";

        public bool IsExpanded {
            get => _isExpanded;
            set {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public ObservableCollection<RevitServerInfo> Children {
            get => _children;
            private set {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        protected async virtual Task<ObservableCollection<RevitServerInfo>> GetChildren(CancellationToken cancellationToken = default) {
            RevitContents contents = await _revitServerClient.GetRootContentsAsync(cancellationToken);
            return GetChildren(contents);
        }

        protected ObservableCollection<RevitServerInfo> GetChildren(RevitContents contents) {
            IEnumerable<RevitServerInfo> revitFolders = contents.Folders.Select(item => new RevitFolderInfo(_revitServerClient) { RevitContents = contents, RevitResponse = item });
            //IEnumerable<RevitServerInfo> revitModels = contents.Models.Select(item => new RevitModelInfo(_revitServerClient) { RevitContents = contents, RevitResponse = item });
            //return new ObservableCollection<RevitServerInfo>(revitFolders.Union(revitModels));
            
            return new ObservableCollection<RevitServerInfo>(revitFolders);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}