using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using dosymep.Bim4Everyone;
using dosymep.Revit.ServerClient;

using Ninject;

using RevitRefreshLinks.Services;
using RevitRefreshLinks.ViewModels;
using RevitRefreshLinks.Views;

namespace RevitRefreshLinks.Extensions;
internal static class NinjectExtensions {
    /// <summary>
    /// Добавляет в контейнер <see cref="IOpenFolderDialog"/>.
    /// </summary>
    /// <param name="kernel">Ninject контейнер.</param>
    /// <param name="title">Заголовок окна. По умолчанию "Выбрать папку на RS".</param>
    /// <param name="initialDirectory">Директория, открываемая по умолчанию.</param>
    /// <param name="multiSelect">True - разрешает мультивыбор. По умолчанию отключено.</param>
    /// <returns>Возвращает настроенный контейнер Ninject.</returns>
    /// <exception cref="System.ArgumentNullException">kernel is null.</exception>
    public static IKernel UseRsOpenFolderDialog(this IKernel kernel,
        string title = "Выбрать папку на RS",
        string initialDirectory = default,
        bool multiSelect = false) {
        if(kernel == null) {
            throw new ArgumentNullException(nameof(kernel));
        }

        kernel.Bind<IFileSystem>()
            .To<RsFileSystem>()
            .WhenInjectedInto<DirectoriesExplorerViewModel>();

        kernel.Bind<DirectoriesExplorerViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<DirectoriesExplorerWindow>()
            .ToSelf()
            .InTransientScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<DirectoriesExplorerViewModel>());
        kernel.Bind<IDirectoriesExplorerWindowProvider>()
            .To<ExplorerWindowProvider>()
            .InSingletonScope();

        kernel.Bind<IReadOnlyCollection<IServerClient>>()
            .ToMethod(c => c.Kernel.Get<Autodesk.Revit.ApplicationServices.Application>()
                .GetRevitServerNetworkHosts()
                .Select(item => new ServerClientBuilder()
                .SetServerName(item)
                .SetServerVersion(ModuleEnvironment.RevitVersion)
                .Build())
            .ToArray());

        kernel.Bind<IOpenFolderDialog>()
            .To<RsOpenFolderDialog>()
            .WithPropertyValue(nameof(IOpenFolderDialog.Title), title)
            .WithPropertyValue(nameof(IOpenFolderDialog.InitialDirectory), initialDirectory)
            .WithPropertyValue(nameof(IOpenFolderDialog.MultiSelect), multiSelect);

        return kernel;
    }

    /// <summary>
    /// Добавляет в контейнер <see cref="IOpenFileDialog"/>.
    /// </summary>
    /// <param name="kernel">Ninject контейнер.</param>
    /// <param name="title">Заголовок окна. По умолчанию "Выбрать файлы с RS".</param>
    /// <param name="initialDirectory">Директория, открываемая по умолчанию.</param>
    /// <param name="multiSelect">True - разрешает мультивыбор. По умолчанию отключено.</param>
    /// <returns>Возвращает настроенный контейнер Ninject.</returns>
    /// <exception cref="System.ArgumentNullException">kernel is null.</exception>
    public static IKernel UseRsOpenFileDialog(this IKernel kernel,
        string title = "Выберите файлы с RS",
        string initialDirectory = default,
        bool multiSelect = false,
        string filter = "*.rvt") {
        if(kernel == null) {
            throw new ArgumentNullException(nameof(kernel));
        }

        kernel.Bind<IFileSystem>()
            .To<RsFileSystem>()
            .WhenInjectedInto<FilesExplorerViewModel>();

        kernel.Bind<FilesExplorerViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FilesExplorerWindow>()
            .ToSelf()
            .InTransientScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<FilesExplorerViewModel>());
        kernel.Bind<IFilesExplorerWindowProvider>()
            .To<ExplorerWindowProvider>()
            .InSingletonScope();

        kernel.Bind<IReadOnlyCollection<IServerClient>>()
            .ToMethod(c => c.Kernel.Get<Autodesk.Revit.ApplicationServices.Application>()
                .GetRevitServerNetworkHosts()
                .Select(item => new ServerClientBuilder()
                .SetServerName(item)
                .SetServerVersion(ModuleEnvironment.RevitVersion)
                .Build())
            .ToArray());

        kernel.Bind<IOpenFileDialog>()
            .To<RsOpenFileDialog>()
            .WithPropertyValue(nameof(IOpenFileDialog.Title), title)
            .WithPropertyValue(nameof(IOpenFileDialog.InitialDirectory), initialDirectory)
            .WithPropertyValue(nameof(IOpenFileDialog.MultiSelect), multiSelect)
            .WithPropertyValue(nameof(IOpenFileDialog.Filter), filter);

        return kernel;
    }
}
