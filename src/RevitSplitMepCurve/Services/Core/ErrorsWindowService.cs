using System;

using RevitSplitMepCurve.Views;

namespace RevitSplitMepCurve.Services.Core;

/// <summary>Тонкая обёртка над ErrorsWindow для немодального показа.</summary>
internal class ErrorsWindowService {
    private readonly ErrorsWindow _errorsWindow;
    private readonly IErrorsService _errorsService;

    public ErrorsWindowService(ErrorsWindow errorsWindow, IErrorsService errorsService) {
        _errorsWindow = errorsWindow ?? throw new ArgumentNullException(nameof(errorsWindow));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
    }

    public void ShowErrorsWindow() {
        if(_errorsService.ContainsErrors()) {
            _errorsWindow.Show();
        }
    }
}
