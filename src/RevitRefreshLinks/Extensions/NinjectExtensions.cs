using System;

using Ninject;

using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.Extensions {
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
            bool multiSelect = false) {
            if(kernel == null) {
                throw new ArgumentNullException(nameof(kernel));
            }

            kernel.Bind<IOpenFileDialog>()
                .To<RsOpenFileDialog>()
                .WithPropertyValue(nameof(IOpenFileDialog.Title), title)
                .WithPropertyValue(nameof(IOpenFileDialog.InitialDirectory), initialDirectory)
                .WithPropertyValue(nameof(IOpenFileDialog.MultiSelect), multiSelect);

            return kernel;
        }
    }
}
