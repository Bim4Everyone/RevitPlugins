using System;

using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement.Services {
    /// <summary>
    /// Класс для назначения используемых связей через выбор пользователя
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
}
