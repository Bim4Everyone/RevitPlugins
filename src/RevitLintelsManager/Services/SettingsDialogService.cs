using System;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitLintelsManager.ViewModels;
using RevitLintelsManager.Views;

namespace RevitLintelsManager.Services;

internal class SettingsDialogService(IResolutionRoot resolutionRoot) {
    public Task<string> RequestNameAsync(string title, string initialName, Func<string, bool> validate) {
        var window = resolutionRoot.Get<NameInputWindow>();
        var viewModel = (NameInputViewModel) window.DataContext;
        viewModel.Initialize(title, initialName, validate);
        window.Owner = resolutionRoot.Get<SettingsWindow>();

        string result = window.ShowDialog() == true ? viewModel.Name.Trim() : null;
        return Task.FromResult(result);
    }
}
