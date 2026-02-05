using System;

using RevitOpeningPlacement.Views.Utils;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Обновляет используемые типы связей в <see cref="Models.RevitRepository"/> по выбору пользователя
/// </summary>
internal class UserSelectedLinksSetter : IRevitLinkTypesSetter {
    private readonly LinksSelectorWindow _window;

    public UserSelectedLinksSetter(LinksSelectorWindow window) {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }


    public void SetRevitLinkTypes() {
        bool success = _window.ShowDialog() ?? false;
        if(!success) {
            throw new OperationCanceledException();
        }
    }
}
