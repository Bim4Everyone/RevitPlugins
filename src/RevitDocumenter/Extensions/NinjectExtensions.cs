using Ninject;

using RevitDocumenter.Models.Mapping.ViewServices;
using RevitDocumenter.Models.MapServices;

namespace RevitDocumenter.Extensions;

internal static class NinjectExtensions {
    public static IKernel BindMapping(this IKernel kernel) {
        kernel.Bind<ViewPreparer>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<AnchorLineService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ImageService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PaintSquaresByMapService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ViewMapService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<BallCreator>()
            .ToSelf()
            .InSingletonScope();

        return kernel;
    }
}
