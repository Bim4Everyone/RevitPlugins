using dosymep.WpfUI.Core;

namespace RevitArchitecturalDocumentation.Models.Services;
public interface IWindowService {
    void Show<TWindow, TViewModel>(TViewModel viewModel)
        where TWindow : WpfUIPlatformWindow;

    bool? ShowDialog<TWindow, TViewModel>(TViewModel viewModel)
        where TWindow : WpfUIPlatformWindow;
}
