﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.Model;
using RevitPlatformSettings.Services;

namespace RevitPlatformSettings.ViewModels.Settings;

internal class ExtensionsSettingsViewModel : SettingsViewModel {
    private readonly IExtensionsService<BuiltinExtension> _builtinService;
    private readonly IExtensionViewModelFactory _extensionFactory;
    private readonly IExtensionsService<ThirdPartyExtension> _thirdPartyService;

    public ExtensionsSettingsViewModel(
        IExtensionViewModelFactory extensionFactory,
        IExtensionsService<BuiltinExtension> builtinService,
        IExtensionsService<ThirdPartyExtension> thirdPartyService) {
        _extensionFactory = extensionFactory;
        _builtinService = builtinService;
        _thirdPartyService = thirdPartyService;

        Extensions =
            new ObservableCollection<ExtensionViewModel>(GetExtensions()
                .Select(item => _extensionFactory.Create(item)));
    }

    public ObservableCollection<ExtensionViewModel> Extensions { get; }

    public override void SaveSettings() {
        foreach(ExtensionViewModel extensionViewModel in Extensions) {
            extensionViewModel.SaveExtensionState();
        }
    }

    private IEnumerable<Extension> GetExtensions() {
        return _thirdPartyService.GetExtensions()
            .OfType<Extension>()
            .Union(_builtinService.GetExtensions());
    }
}
