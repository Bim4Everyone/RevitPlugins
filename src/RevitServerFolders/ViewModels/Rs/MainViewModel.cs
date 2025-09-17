using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


using dosymep.Revit.ServerClient;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitServerFolders.ViewModels.Rs;
internal sealed class MainViewModel : BaseViewModel {
    private readonly IReadOnlyCollection<IServerClient> _serverClients;
    private readonly ILocalizationService _localization;
    private RsModelObjectViewModel _selectedItem;
    private ObservableCollection<RsModelObjectViewModel> _items;

    private string _errorText;

    public MainViewModel(IReadOnlyCollection<IServerClient> serverClients, ILocalizationService localization) {
        _serverClients = serverClients;
        _localization = localization;
        LoadViewCommand = RelayCommand.CreateAsync(LoadView);
        AcceptViewCommand = RelayCommand.CreateAsync(AcceptView, CanAcceptView);

        LoadChildrenCommand = RelayCommand.CreateAsync<RsModelObjectViewModel>(LoadChildren, CanLoadChildren);
        ReloadChildrenCommand = RelayCommand.CreateAsync(ReloadChildren, CanReloadChildren);
    }

    public IAsyncCommand LoadViewCommand { get; }
    public IAsyncCommand AcceptViewCommand { get; }
    public IAsyncCommand ReloadChildrenCommand { get; }
    public IAsyncCommand LoadChildrenCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public RsModelObjectViewModel SelectedItem {
        get => _selectedItem;
        set => RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<RsModelObjectViewModel> Items {
        get => _items;
        set => RaiseAndSetIfChanged(ref _items, value);
    }

    public void CancelCommands() {
        if(Items != null) {
            foreach(var item in Items) {
                item?.Cancel();
            }
        }
    }

    public void RemoveCancellation() {
        if(Items != null) {
            foreach(var item in Items) {
                item?.RemoveCancellation();
            }
        }
    }

    private Task LoadView() {
        Items ??= new ObservableCollection<RsModelObjectViewModel>(
                _serverClients
                    .Select(item => new RsServerDataViewModel(item)));

        return Task.CompletedTask;
    }

    private Task AcceptView() {
        return Task.CompletedTask;
    }

    private bool CanAcceptView() {
        if(SelectedItem is not RsFolderDataViewModel) {
            ErrorText = _localization.GetLocalizedString("RsBrowser.Validation.SelectFolder");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private async Task ReloadChildren() {
        await SelectedItem.ReloadChildrenCommand.ExecuteAsync(default);
    }

    private bool CanReloadChildren() {
        return SelectedItem != null && SelectedItem.ReloadChildrenCommand.CanExecute(default);
    }

    private async Task LoadChildren(RsModelObjectViewModel model) {
        await model.LoadChildrenCommand.ExecuteAsync(default);
    }

    private bool CanLoadChildren(RsModelObjectViewModel model) {
        return model is not null && model.LoadChildrenCommand.CanExecute(default);
    }
}
