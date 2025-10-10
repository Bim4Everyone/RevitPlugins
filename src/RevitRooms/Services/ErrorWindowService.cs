using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms.Services;

internal class ErrorWindowService {
    private readonly IResolutionRoot _resolutionRoot;

    public ErrorWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public bool ShowNoticeWindow(string title, 
                                 bool notShowWarnings,
                                 IEnumerable<InfoElementViewModel> infoElements) {
        if(notShowWarnings) {
            infoElements = infoElements.Where(item => item.TypeInfo != TypeInfo.Warning);
        }

        if(infoElements.Any()) {
            var window = new InfoElementsWindow() {
                Title = title,
                DataContext = new InfoElementsViewModel() {
                    InfoElement = infoElements.FirstOrDefault(),
                    InfoElements = [.. infoElements]
                }
            };

            window.Show();
            return true;
        }

        return false;
    }
}
