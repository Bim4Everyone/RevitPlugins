using System;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class NewEntitySaver : IEntitySaver {
        public void Save(SheetModel sheetModel) {
            throw new NotImplementedException();
            //if(State == EntityState.Deleted && _viewSheet is not null) {
            //    repository.DeleteElement(_viewSheet.Id);

            //} else if(State == EntityState.Modified && _viewSheet is not null) {
            //    repository.UpdateViewSheet(_viewSheet, this);
            //    SaveNestedItems(repository);
            //    State = EntityState.Unchanged;
            //} else if(State == EntityState.Added
            //    || State == EntityState.Modified && _viewSheet is null) {
            //    if(TitleBlockSymbol is null) {
            //        throw new InvalidOperationException(
            //            $"Перед сохранением необходимо назначить {nameof(TitleBlockSymbol)}");
            //    }
            //    _viewSheet = repository.CreateViewSheet(this);
            //    SaveNestedItems(repository);
            //    State = EntityState.Unchanged;
            //} else {
            //    SaveNestedItems(repository);
            //}
        }

        public void Save(ViewPortModel viewModel) {
            throw new NotImplementedException();
            //if(State == EntityState.Deleted && _viewport is not null) {
            //    repository.DeleteElement(_viewport.Id);

            //} else if(State == EntityState.Modified && _viewport is not null) {
            //    if(ViewPortType is null) {
            //        throw new InvalidOperationException(
            //            $"Перед сохранением измененного видового экрана необходимо назначить {nameof(ViewPortType)}");
            //    }
            //    repository.UpdateViewPort(_viewport, ViewPortType.Id);
            //    State = EntityState.Unchanged;

            //} else if(State == EntityState.Added && _view is not null) {
            //    if(Location is null) {
            //        throw new InvalidOperationException(
            //            $"Перед сохранением нового видового экрана необходимо назначить {nameof(Location)}");
            //    }
            //    if(ViewPortType is null) {
            //        throw new InvalidOperationException(
            //            $"Перед сохранением нового видового экрана необходимо назначить {nameof(ViewPortType)}");
            //    }
            //    repository.CreateViewPort(Sheet.GetViewSheet().Id, _view.Id, ViewPortType.Id, Location);
            //    State = EntityState.Unchanged;
            //}
        }

        public void Save(ScheduleModel scheduleModel) {
            throw new NotImplementedException();
            //if(State == EntityState.Deleted && _scheduleInstance is not null) {
            //    repository.DeleteElement(_scheduleInstance.Id);
            //} else if(State == EntityState.Added && _schedule is not null) {
            //    if(Location is null) {
            //        throw new InvalidOperationException(
            //            $"Перед сохранением новой спецификации необходимо назначить {nameof(Location)}");
            //    }
            //    repository.CreateSchedule(Sheet.GetViewSheet().Id, _schedule.Id, Location);
            //    State = EntityState.Unchanged;
            //}
        }

        public void Save(AnnotationModel annotationModel) {
            throw new NotImplementedException();
            //if(State == EntityState.Deleted && _annotationSymbol is not null) {
            //    repository.DeleteElement(_annotationSymbol.Id);
            //} else if(State == EntityState.Added && _annotationType is not null) {
            //    if(Location is null) {
            //        throw new InvalidOperationException(
            //            $"Перед сохранением новой аннотации необходимо назначить {nameof(Location)}");
            //    }
            //    repository.CreateAnnotation(Sheet.GetViewSheet(), _annotationType, Location);
            //    State = EntityState.Unchanged;
            //}
        }
    }
}
