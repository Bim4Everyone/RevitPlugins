using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitRooms.Models;
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
                                 IEnumerable<WarningViewModel> allWarnings) {
        if(notShowWarnings) {
            allWarnings = allWarnings
                .Where(item => item.TypeInfo != WarningTypeInfo.Warning)
                .OrderBy(x => x.TypeInfo);
        }

        if(allWarnings.Any()) {
            bool hasErrors = allWarnings
                .Where(item => item.TypeInfo == WarningTypeInfo.Error)
                .Any();
            bool hasWarnings = allWarnings
                .Where(item => item.TypeInfo == WarningTypeInfo.Warning)
                .Any();

            string message;
            if(hasErrors && hasWarnings) {
                message = "Расчет не выполнен, так как есть ошибки и предупреждения.\r\nОшибки обязательны к исправлению.\r\nПредупреждения следует проанализировать и при необходимости исправить.";
            } else if(hasErrors) {
                message = "Расчет не выполнен, так как есть ошибки.\r\nОшибки обязательны к исправлению.";
            } else {
                message = "Расчет завершен с предупреждениями.\r\nПредупреждения следует проанализировать и при необходимости исправить.";
            }

            var window = _resolutionRoot.Get<WarningsWindow>();
            window.Title = title;
            window.DataContext = new WarningsViewModel() {
                Description = message,
                Warnings = [.. allWarnings.OrderBy(x => x.TypeInfo)]
            };

            window.Show();
            return true;
        }

        return false;
    }
}
