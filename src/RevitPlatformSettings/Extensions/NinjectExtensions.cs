using Ninject;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.Model;
using RevitPlatformSettings.Services;
using RevitPlatformSettings.ViewModels.Settings;
using RevitPlatformSettings.Views.Pages;

namespace RevitPlatformSettings.Extensions;

internal static class NinjectExtensions {
    public static IKernel BindPages(this IKernel kernel) {
        kernel.Bind<SettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<GeneralSettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ExtensionsSettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<RevitParamsSettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<TelemetrySettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<AboutSettingsPage>()
            .ToSelf()
            .InSingletonScope();

        return kernel;
    }

    public static IKernel BindViewModels(this IKernel kernel) {
        kernel.Bind<SettingsViewModel>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<GeneralSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ExtensionsSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<RevitParamsSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<TelemetrySettingsViewModel>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<AboutSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();


        return kernel;
    }

    public static IKernel BindExtensions(this IKernel kernel) {
        kernel.Bind<BuiltinExtension>().ToSelf();
        kernel.Bind<ThirdPartyExtension>().ToSelf();

        kernel.Bind<IExtensionFactory<BuiltinExtension>>()
            .To<ExtensionFactory<BuiltinExtension>>();

        kernel.Bind<IExtensionFactory<ThirdPartyExtension>>()
            .To<ExtensionFactory<ThirdPartyExtension>>();

        kernel.Bind<IExtensionsService<BuiltinExtension>>()
            .To<BuiltinExtensionsService>();

        kernel.Bind<IExtensionsService<ThirdPartyExtension>>()
            .To<ThirdPartyExtensionsService>();

        kernel.Bind<IPyRevitExtensionsService>()
            .To<PyRevitExtensionsService>();

        kernel.Bind<IExtensionViewModelFactory>()
            .To<ExtensionViewModelFactory>();

        return kernel;
    }
}
