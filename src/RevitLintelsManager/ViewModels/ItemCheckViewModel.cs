namespace RevitLintelsManager.ViewModels;

internal class ItemCheckViewModel : ItemViewModel {
    private bool _isChecked;
    
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
}
