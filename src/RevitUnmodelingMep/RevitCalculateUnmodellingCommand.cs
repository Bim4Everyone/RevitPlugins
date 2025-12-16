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
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitUnmodelingMep.Models;
using RevitUnmodelingMep.ViewModels;
using RevitUnmodelingMep.Views;

namespace RevitUnmodelingMep {
    [Transaction(TransactionMode.Manual)]
    public class RevitCalculateUnmodellingCommand : BasePluginCommand {
        public RevitCalculateUnmodellingCommand() {
            PluginName = "Полное обновление видимого";
        }

        protected override void Execute(UIApplication uiApplication) {
            // Создание контейнера зависимостей плагина с сервисами из платформы
            using IKernel kernel = uiApplication.CreatePlatformServices();

            // Настройка доступа к Revit
            kernel.Bind<RevitRepository>()
                .ToSelf()
                .InSingletonScope();

            kernel.Bind<Document>()
                .ToMethod(ctx => ctx.Kernel.Get<UIApplication>().ActiveUIDocument.Document)
                .InSingletonScope();

            kernel.Bind<SettingsUpdater>()
                .ToSelf()
                .InSingletonScope();

            // Настройка конфигурации плагина
            kernel.Bind<PluginConfig>()
                .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

            var repository = kernel.Get<RevitRepository>();
            repository.CalculateUnmodeling();
        }
    }
}
