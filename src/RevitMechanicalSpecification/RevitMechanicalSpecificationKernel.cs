using System;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using Ninject;

using RevitMechanicalSpecification.Models;
using RevitMechanicalSpecification.Service;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitMechanicalSpecification {
    internal static class RevitMechanicalSpecificationKernel {
        public static IKernel Create(UIApplication uiApplication) {
            IKernel kernel = new StandardKernel();
            Document document = uiApplication.ActiveUIDocument.Document;

            kernel.Bind<UIApplication>()
                .ToConstant(uiApplication)
                .InTransientScope();

            kernel.Bind<Application>()
                .ToConstant(uiApplication.Application)
                .InTransientScope();

            kernel.Bind<Document>()
                .ToConstant(document)
                .InSingletonScope();

            kernel.Bind<IMessageBoxService>()
                .ToMethod(c => ServicesProvider.GetPlatformService<IMessageBoxService>())
                .InSingletonScope();

            kernel.Bind<SpecConfiguration>()
                .ToSelf()
                .InSingletonScope();

            kernel.Bind<ElementProcessor>()
                .ToSelf()
                .InSingletonScope();

            kernel.Bind<RevitRepository>()
                .ToSelf()
                .InSingletonScope();
            
            var messageBoxService = kernel.Get<IMessageBoxService>();
            CheckDocument(document, messageBoxService);

            return kernel;
        }
        
        private static void CheckDocument(Document document, IMessageBoxService service) {
            if(document.IsFamilyDocument) {
                ShowErrorAndCancel(service);
            }
        }

        private static void ShowErrorAndCancel(
            IMessageBoxService service) {

            service.Show(
                "Плагин не предназначен для работы с семействами",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            throw new OperationCanceledException();
        }
    }
}
