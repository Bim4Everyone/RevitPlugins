using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.Revit.ServerClient;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;

namespace RevitServerFolders.ViewModels.Rs {
    internal class RsModelObjectViewModel : BaseViewModel {
        protected readonly IServerClient _serverClient;

        private string _size;
        private ObservableCollection<RsModelObjectViewModel> _children;

        private bool _isExpanded;
        private bool _isLoadedChildren;

        protected RsModelObjectViewModel(IServerClient serverClient) {
            _serverClient = serverClient;

            Children = new ObservableCollection<RsModelObjectViewModel>() {null};
            LoadChildrenCommand = RelayCommand.CreateAsync(LoadChildrenObjects, CanLoadChildrenObjects);
            ReloadChildrenCommand = RelayCommand.CreateAsync(ReLoadChildrenObjects, CanReLoadChildrenObjects);
        }

        public AsyncRelayCommand LoadChildrenCommand { get; }
        public AsyncRelayCommand ReloadChildrenCommand { get; }

        public virtual string Name => "";
        public virtual string FullName => "";
        public virtual bool HasChildren => false;

        public virtual string Version => _serverClient.ServerVersion;

        public string Size {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }
        
        public bool IsExpanded {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool IsLoadedChildren {
            get => _isLoadedChildren;
            set => this.RaiseAndSetIfChanged(ref _isLoadedChildren, value);
        }

        public ObservableCollection<RsModelObjectViewModel> Children {
            get => _children;
            set => this.RaiseAndSetIfChanged(ref _children, value);
        }
        
        public virtual ModelObject GetModelObject() {
            return default;
        }
        
        protected virtual Task<IEnumerable<RsModelObjectViewModel>> GetChildrenObjects() {
            return Task.FromResult(Enumerable.Empty<RsModelObjectViewModel>());
        }
        
        private async Task LoadChildrenObjects() {
            try {
                Children = new ObservableCollection<RsModelObjectViewModel>(await GetChildrenObjects());
            } finally {
                IsLoadedChildren = true;
            }
        }

        private bool CanLoadChildrenObjects() {
            return !IsLoadedChildren;
        }
        
        private async Task ReLoadChildrenObjects() {
            Children.Clear();
            IsLoadedChildren = false;
            await LoadChildrenObjects();
        }

        private bool CanReLoadChildrenObjects() {
            return IsLoadedChildren;
        }
    }
}
