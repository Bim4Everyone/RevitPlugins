using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using DevExpress.Xpf.Grid.TreeList;

using dosymep.Revit.ServerClient;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitServerFolders.ViewModels.Rs {
    internal sealed class MainViewModel : BaseViewModel {
        private readonly IReadOnlyCollection<IServerClient> _serverClients;

        private RsModelObjectViewModel _selectedItem;
        private ObservableCollection<RsModelObjectViewModel> _items;

        private string _errorText;

        public MainViewModel(IReadOnlyCollection<IServerClient> serverClients) {
            _serverClients = serverClients;
            LoadViewCommand = RelayCommand.CreateAsync(LoadView);
            AcceptViewCommand = RelayCommand.CreateAsync(AcceptView, CanAcceptView);
            
            LoadChildrenCommand = RelayCommand.CreateAsync<TreeListNodeEventArgs>(LoadChildren, CanLoadChildren);
            ReloadChildrenCommand = RelayCommand.CreateAsync(ReloadChildren, CanReloadChildren);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        
        public ICommand LoadChildrenCommand { get; }
        public ICommand ReloadChildrenCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public RsModelObjectViewModel SelectedItem {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public ObservableCollection<RsModelObjectViewModel> Items {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        private Task LoadView() {
            Items = new ObservableCollection<RsModelObjectViewModel>(
                _serverClients
                    .Select(item => new RsServerDataViewModel(item)));

            return Task.CompletedTask;
        }

        private Task AcceptView() {
            return Task.CompletedTask;
        }

        private bool CanAcceptView() {
            return SelectedItem is RsFolderDataViewModel;
        }

        private async Task LoadChildren(TreeListNodeEventArgs args) {
            if(args.Row is RsModelObjectViewModel rsModelObject) {
                await rsModelObject.LoadChildrenCommand.ExecuteAsync(default);
            }
        }

        private bool CanLoadChildren(TreeListNodeEventArgs args) {
            return args.Row is RsModelObjectViewModel rsModelObject
                   && rsModelObject.LoadChildrenCommand.CanExecute(default);
        }
        
        private async Task ReloadChildren() {
            await SelectedItem.ReloadChildrenCommand.ExecuteAsync(default);
        }

        private bool CanReloadChildren() {
            return SelectedItem != null && SelectedItem.ReloadChildrenCommand.CanExecute(default);
        }
    }
}
