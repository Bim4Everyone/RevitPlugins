using System.ComponentModel;

namespace RevitClashDetective.Resources;

internal interface IWindowClosingHandler {
    void OnWindowClosing(CancelEventArgs e);
}
