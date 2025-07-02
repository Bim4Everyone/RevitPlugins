using System.Linq;

using Ninject;
using Ninject.Syntax;

using RevitFinishing.ViewModels.Notices;
using RevitFinishing.Views;

namespace RevitFinishing.Services;
internal class ErrorWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public ErrorWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public bool ShowNoticeWindow(NoticeViewModel viewModel) {
        if(viewModel.NoticeLists.Any()) {
            ErrorsWindow window = _resolutionRoot.Get<ErrorsWindow>();
            window.DataContext = viewModel;
            window.Show();

            return true;
        }
        return false;
    }
}
