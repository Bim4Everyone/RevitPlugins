using System;
using System.Windows;

using dosymep.SimpleServices;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal class UserMessageException : OperationCanceledException {
    public UserMessageException(string message)
        : base(message) {
    }

    public static void Throw(IMessageBoxService messageBoxService, string title, string message) {
        messageBoxService?.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        throw new UserMessageException(title);
    }
}
