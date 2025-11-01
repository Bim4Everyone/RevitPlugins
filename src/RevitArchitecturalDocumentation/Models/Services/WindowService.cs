using dosymep.WpfUI.Core;

using Ninject;

namespace RevitArchitecturalDocumentation.Models.Services;
public class WindowService : IWindowService {
    private readonly IKernel _kernel;

    public WindowService(IKernel kernel) {
        _kernel = kernel;
    }

    public void Show<TWindow, TViewModel>(TViewModel viewModel)
        where TWindow : WpfUIPlatformWindow {
        var window = _kernel.Get<TWindow>();
        window.DataContext = viewModel;
        window.Show();
    }

    public bool? ShowDialog<TWindow, TViewModel>(TViewModel viewModel)
        where TWindow : WpfUIPlatformWindow {
        var window = _kernel.Get<TWindow>();
        window.DataContext = viewModel;
        return window.ShowDialog();
    }
}
