using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitParamsChecker.ViewModels.Utils;
using RevitParamsChecker.Views.Utils;

namespace RevitParamsChecker.Services;

internal class NameEditorService {
    private readonly IResolutionRoot _resolutionRoot;

    public NameEditorService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
    }

    public async Task<ICollection<string>> SelectNames(string prompt, string[] allNames, string[] selectedNames) {
        var allNamesArg = new ConstructorArgument(nameof(allNames), allNames);
        var selectedNamesArg = new ConstructorArgument(nameof(selectedNames), selectedNames);
        var vm = _resolutionRoot.Get<SelectableNamesViewModel>(allNamesArg, selectedNamesArg);
        vm.Title = prompt;

        var dialog = _resolutionRoot.Get<SelectableNamesDialog>();
        dialog.DataContext = vm;
        var dialogResult = await dialog.ShowAsync();
        if(dialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary) {
            return vm.GetSelectedEntities();
        } else {
            throw new OperationCanceledException();
        }
    }

    public string CreateNewName(string prompt, string[] existingNames) {
        var existingNamesArg = new ConstructorArgument(nameof(existingNames), existingNames);
        var vm = _resolutionRoot.Get<NameEditorViewModel>(existingNamesArg);
        vm.Title = prompt;

        var window = _resolutionRoot.Get<NameEditorWindow>();
        window.DataContext = vm;
        if(window.ShowDialog() == true) {
            return vm.Name;
        } else {
            throw new OperationCanceledException();
        }
    }

    public string CreateNewName(string prompt, string[] existingNames, string currentName) {
        var existingNamesArg = new ConstructorArgument(nameof(existingNames), existingNames);
        var vm = _resolutionRoot.Get<NameEditorViewModel>(existingNamesArg);
        vm.Title = prompt;
        vm.Name = currentName;

        var window = _resolutionRoot.Get<NameEditorWindow>();
        window.DataContext = vm;
        if(window.ShowDialog() == true) {
            return vm.Name;
        } else {
            throw new OperationCanceledException();
        }
    }
}
