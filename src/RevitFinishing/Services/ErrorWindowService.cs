using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitFinishing.ViewModels;
using RevitFinishing.Views;

namespace RevitFinishing.Services
{
    internal class ErrorWindowService {
        private readonly IResolutionRoot _resolutionRoot;

        public ErrorWindowService(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot;
        }

        public bool ShowNoticeWindow(NoticeViewModel viewModel) {
            if(viewModel.NoticeLists.Any()) {
                var window = _resolutionRoot.Get<ErrorsWindow>();
                window.DataContext = viewModel;
                window.Show();

                return true;
            }
            return false;
        }
    }
}
