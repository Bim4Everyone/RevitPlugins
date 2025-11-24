using Ninject;

using RevitPylonDocumentation.Views.Pages;

namespace RevitPylonDocumentation.Extensions;

internal static class NinjectExtensions {
    public static IKernel BindPages(this IKernel kernel) {
        kernel.Bind<GeneralPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<VerticalViewSettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<HorizontalViewSettingsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<SchedulesPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<LegendViewsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PylonParamsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ProjectParamsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<SheetParamsPage>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<AboutPage>()
            .ToSelf()
            .InSingletonScope();

        return kernel;
    }

    //public static IKernel BindViewModels(this IKernel kernel) {
    //    kernel.Bind<GeneralVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<VerticalViewSettingsVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<HorizontalViewSettingsVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<SchedulesVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<LegendViewsVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<PylonParamsVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<ProjectParamsVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<SheetParamsVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    kernel.Bind<AboutVM>()
    //        .ToSelf()
    //        .InSingletonScope();

    //    return kernel;
    //}

    //public static IKernel BindExtensions(this IKernel kernel) {
    //    kernel.Bind<BuiltinExtension>().ToSelf();
    //    kernel.Bind<ThirdPartyExtension>().ToSelf();

    //    kernel.Bind<IExtensionFactory<BuiltinExtension>>()
    //        .To<ExtensionFactory<BuiltinExtension>>();

    //    kernel.Bind<IExtensionFactory<ThirdPartyExtension>>()
    //        .To<ExtensionFactory<ThirdPartyExtension>>();

    //    kernel.Bind<IExtensionsService<BuiltinExtension>>()
    //        .To<BuiltinExtensionsService>();

    //    kernel.Bind<IExtensionsService<ThirdPartyExtension>>()
    //        .To<ThirdPartyExtensionsService>();

    //    kernel.Bind<IPyRevitExtensionsService>()
    //        .To<PyRevitExtensionsService>();

    //    kernel.Bind<IExtensionViewModelFactory>()
    //        .To<ExtensionViewModelFactory>();

    //    return kernel;
    //}
}
