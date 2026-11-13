using dosymep.WPF.ViewModels;

namespace RevitDocumenter.ViewModels;

internal class ReferenceNameViewModel : BaseViewModel {
    private bool _isCheck;
    private string _referenceName;

    public ReferenceNameViewModel(string referenceName) {
        ReferenceName = referenceName;
    }

    /// <summary>
    /// Указывает выбран ли пользователем данный тип конструкции в интерфейсе для работы
    /// </summary>
    public bool IsCheck {
        get => _isCheck;
        set => RaiseAndSetIfChanged(ref _isCheck, value);
    }

    /// <summary>
    /// Имя опорной плоскости
    /// </summary>
    public string ReferenceName {
        get => _referenceName;
        set => RaiseAndSetIfChanged(ref _referenceName, value);
    }
}
