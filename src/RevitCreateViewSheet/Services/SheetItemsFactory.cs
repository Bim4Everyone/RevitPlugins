using System;
using System.Linq;

using Ninject;
using Ninject.Syntax;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.ViewModels;
using RevitCreateViewSheet.Views;

namespace RevitCreateViewSheet.Services {
    internal class SheetItemsFactory {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly EntitySaverProvider _entitySaverProvider;

        public SheetItemsFactory(IResolutionRoot resolutionRoot, EntitySaverProvider entitySaverProvider) {
            _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
            _entitySaverProvider = entitySaverProvider ?? throw new ArgumentNullException(nameof(entitySaverProvider));
        }


        public AnnotationModel CreateAnnotation(SheetModel sheetModel) {
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }

            var window = _resolutionRoot.Get<AnnotationModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var symbol = (window.DataContext as AnnotationModelCreatorViewModel)
                    .SelectedAnnotationSymbolType
                    .AnnotationSymbolType;
                return new AnnotationModel(sheetModel, symbol, _entitySaverProvider.GetNewEntitySaver());
            }
            throw new OperationCanceledException();
        }

        public ScheduleModel CreateSchedule(SheetModel sheetModel) {
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }

            var window = _resolutionRoot.Get<ScheduleModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var creatorView = window.DataContext as ScheduleModelCreatorViewModel;
                var selectedSchedule = creatorView.SelectedViewSchedule;
                creatorView.SelectedViewSchedule = creatorView.ViewSchedules.FirstOrDefault();
                return new ScheduleModel(
                    sheetModel,
                    selectedSchedule.ViewSchedule,
                    _entitySaverProvider.GetNewEntitySaver());
            }
            throw new OperationCanceledException();
        }

        public ViewPortModel CreateViewPort(SheetModel sheetModel) {
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }
            var creatorViewModel = _resolutionRoot.Get<ViewPortModelCreatorViewModel>();
            creatorViewModel.UpdateEnabledViews(sheetModel);
            var window = _resolutionRoot.Get<ViewPortModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var selectedView = creatorViewModel.SelectedView;
                var selectedViewPortType = creatorViewModel.SelectedViewPortType;
                return new ViewPortModel(
                    sheetModel,
                    selectedView.View,
                    selectedViewPortType.ViewType,
                    _entitySaverProvider.GetNewEntitySaver());
            }
            throw new OperationCanceledException();
        }
    }
}
