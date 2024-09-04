using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.ViewModels;
using RevitAxonometryViews.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitAxonometryViews {
    [Transaction(TransactionMode.Manual)]
    public class RevitAxonometryViewsCommand : BasePluginCommand {
        public RevitAxonometryViewsCommand() {
            PluginName = "Создать схемы";
        }
        protected override void Execute(UIApplication uiApplication) {
            // Здесь мы биндим классы в словарь Kernel, который сам будет их инициализировать через Get<Имя из словаря>, без вызова конструкторов
            // Которые он обрабатывает самостоятельно
            // Например Kernel.Get<MainViewModel>() требует на вход RevitRepository. Kernel самостоятельно ищет его по биндингам и подает в конструктор
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InSingletonScope();
                kernel.Bind<RevitRepository>().ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainViewModel>().ToSelf()
                    .InSingletonScope();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(Window.Title), PluginName);

                var servise = GetPlatformService<IMessageBoxService>();
                CheckParameter(uiApplication.ActiveUIDocument.Document, servise);

                var revitRepository = kernel.Get<RevitRepository>();
                Notification(kernel.Get<MainWindow>());
            }
        }

        private void ShowReport(string report, IMessageBoxService service) {
            if(!string.IsNullOrEmpty(report)) {
                service.Show(report, "Генерация схем", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new OperationCanceledException();
            }
        }

        // Проверка нужна до финализации вопроса с параметрами, пока что возможна ситуация без параметра в проекте
        // По добавлению параметров проверка уйдет в репозиторий
        private void CheckParameter(Document document, IMessageBoxService service) {
            string report = string.Empty;
            if(document.IsFamilyDocument) {
                report = "Плагин не предназначен для работы с семействами";
                ShowReport(report, service);
            }

            if(!document.IsExistsParam("ФОП_ВИС_Имя системы")) {
                report = "ФОП_ВИС_Имя системы отсутствует в проекте";
                ShowReport(report, service);
            }
        }
    }
}

