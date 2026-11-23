using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Локализованная подпись диапазона для переключателя, аналогичного RevitRoundingOfAreas.</summary>
internal sealed class ManagerScopeViewModel(ManagerScope scope, ILocalizationService localization) : BaseViewModel {
    public ManagerScope Scope { get; } = scope;
    public string Name => localization.GetLocalizedString($"Manager.{Scope}");

    /// <summary>Обновляет подпись при смене языка, сохраняя выбранный диапазон.</summary>
    public void RefreshLocalization() {
        RaisePropertyChanged(nameof(Name));
    }
}
