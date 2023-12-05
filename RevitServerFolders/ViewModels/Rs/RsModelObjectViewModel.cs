using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.Revit.ServerClient;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitServerFolders.ViewModels.Rs {
    internal class RsModelObjectViewModel : BaseViewModel {
        protected readonly IServerClient _serverClient;

        private string _size;
        private ObservableCollection<RsModelObjectViewModel> _children;

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
            Children = new ObservableCollection<RsModelObjectViewModel>(await GetChildrenObjects());
        }

        private bool CanLoadChildrenObjects() {
            return true;
        }

        protected virtual Task<IEnumerable<RsModelObjectViewModel>> GetChildrenObjects() {
            return Task.FromResult(Enumerable.Empty<RsModelObjectViewModel>());
        }
    }
}
