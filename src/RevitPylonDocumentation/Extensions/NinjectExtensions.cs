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

        return kernel;
    }
}
