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

        private bool _isLoadedChildren;

        protected RsModelObjectViewModel(IServerClient serverClient) {
            _serverClient = serverClient;

            Children = new ObservableCollection<RsModelObjectViewModel>() {null};
            LoadChildrenCommand = RelayCommand.CreateAsync(LoadChildrenObjects, CanLoadChildrenObjects);
        }

        public ICommand LoadChildrenCommand { get; }

        public virtual string Name => "";
        public virtual string FullName => "";
        public virtual bool HasChildren => false;

        public virtual string Version => _serverClient.ServerVersion;

        public string Size {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }

        public ObservableCollection<RsModelObjectViewModel> Children {
            get => _children;
            set => this.RaiseAndSetIfChanged(ref _children, value);
        }

        private async Task LoadChildrenObjects() {
            try {
                Children = new ObservableCollection<RsModelObjectViewModel>(await GetChildrenObjects());
            } finally {
                _isLoadedChildren = true;
            }
        }

        private bool CanLoadChildrenObjects() {
            return !_isLoadedChildren;
        }

        protected virtual Task<IEnumerable<RsModelObjectViewModel>> GetChildrenObjects() {
            return Task.FromResult(Enumerable.Empty<RsModelObjectViewModel>());
        }

        public virtual ModelObject GetModelObject() {
            return default;
        }
    }
}
